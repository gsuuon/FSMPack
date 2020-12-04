module FSMPack.ReadValue

open Microsoft.FSharp.Core

open FSMPack.Spec
open FSMPack.Utility.Byte

open System
open System.Buffers.Binary
open System.Runtime.CompilerServices
open System.Collections.Generic

type BufReader =
    {
        mutable idx : int
    }
    member x.Advance count =
        x.idx <- x.idx + count

let readByte (bufReader: BufReader) (bytes: inref<Bytes>) =
    let idx = bufReader.idx
    bufReader.Advance 1
    bytes.[idx]

let readBytes (bufReader: BufReader) (bytes: inref<Bytes>) count =
    let idx = bufReader.idx
    bufReader.Advance count
    let x = bytes.Slice(idx, count)
    x

let readFormat (byt: byte) =
    let format : Format =
        LanguagePrimitives.EnumOfValue<byte, Format>(byt)

    format

let enumValue = LanguagePrimitives.EnumToValue

let readString (br: BufReader) (bytes: inref<Bytes>) len =
    // netstandard2.1 adds ReadOnlySpan api for Encoding.x.GetString
    (readBytes br &bytes len).ToArray()
    |> Text.Encoding.UTF8.GetString
    |> Value.RawString

let rec readNextValue (br: BufReader) (bytes: inref<Bytes>) =
    // figure out format by header byte
    match readFormat <| readByte br &bytes with
    | Format.Nil -> Value.Nil
    | Format.False -> Value.Boolean false
    | Format.True -> Value.Boolean true
    | Format.UInt8 ->
        readByte br &bytes
        |> int
        |> Value.Integer
    | Format.UInt16 ->
            // Can't pipe into byref function
            // https://github.com/dotnet/fsharp/issues/5286#issuecomment-402249997
        BinaryPrimitives.ReadUInt16BigEndian
            (readBytes br &bytes 2)
        |> int
        |> Value.Integer
    | Format.UInt32 ->
        BinaryPrimitives.ReadUInt32BigEndian
            (readBytes br &bytes 4)
        |> Value.UInteger
    | Format.UInt64 ->
        BinaryPrimitives.ReadUInt64BigEndian
            (readBytes br &bytes 8)
        |> Value.UInteger64
    | Format.Int8 ->
        readByte br &bytes
        |> sbyte
        |> int
        |> Value.Integer
    | Format.Int16 ->
        BinaryPrimitives.ReadInt16BigEndian
            (readBytes br &bytes 2)
        |> int
        |> Value.Integer
    | Format.Int32 ->
        BinaryPrimitives.ReadInt32BigEndian
            (readBytes br &bytes 3)
        |> int
        |> Value.Integer
    | Format.Int64 ->
        BinaryPrimitives.ReadInt64BigEndian
            (readBytes br &bytes 8)
        |> Value.Integer64
    | Format.Str8 ->
        let len =
            readByte br &bytes
            |> int

        readString br &bytes len
    | Format.Str16 ->
        let len =
            BinaryPrimitives.ReadUInt16BigEndian
                (readBytes br &bytes 2)
            |> int

        readString br &bytes len
    | Format.Str32 ->
        let len =
            BinaryPrimitives.ReadUInt32BigEndian
                (readBytes br &bytes 4)
            |> int

        readString br &bytes len
    | Format.Bin8 ->
        let len =
            readByte br &bytes 
            |> int

        (readBytes br &bytes len).ToArray()
        |> Value.RawBinary
    | Format.Bin16 ->
        let len =
            BinaryPrimitives.ReadUInt16BigEndian
                (readBytes br &bytes 2)
            |> int

        (readBytes br &bytes len).ToArray()
        |> Value.RawBinary
    | Format.Bin32 ->
        let len =
            BinaryPrimitives.ReadUInt32BigEndian
                (readBytes br &bytes 4)
            |> int

        (readBytes br &bytes len).ToArray()
        |> Value.RawBinary
    | Format.Array16 ->
        let len =
            BinaryPrimitives.ReadUInt16BigEndian
                (readBytes br &bytes 2)
            |> int

        readArrayValues br &bytes (Stack()) len
    | Format.Array32 ->
        let len =
            BinaryPrimitives.ReadUInt32BigEndian
                (readBytes br &bytes 4)
            |> int

        readArrayValues br &bytes (Stack()) len
    | Format.Map16 ->
        let len =
            BinaryPrimitives.ReadUInt16BigEndian
                (readBytes br &bytes 2)
            |> int

        readMapValues br &bytes (Dictionary()) len
    | Format.Map32 ->
        let len =
            BinaryPrimitives.ReadUInt32BigEndian
                (readBytes br &bytes 4)
            |> int
        readMapValues br &bytes (Dictionary()) len
    | format ->
        match enumValue format with
        | byt when
            format >= Format.PositiveFixInt &&
            byt <= 0x7fuy ->

            byt
            |> int
            |> Value.Integer

        | byt when
            format > Format.FixMap &&
            byt <= 0x8fuy ->

            let len =
                maskByte 0b11110000 byt
                |> int

            readMapValues br &bytes (Dictionary()) len

        | byt when
            format > Format.FixArray &&
            byt <= 0x9fuy ->

            let len =
                maskByte 0b11110000uy byt
                |> int

            readArrayValues br &bytes (Stack()) len

        | byt when
            format >= Format.FixStr &&
            byt <= 0xbfuy ->

            let len =
                maskByte 0b11100000uy byt
                |> int

            (readBytes br &bytes len).ToArray()
            |> Text.Encoding.UTF8.GetString
            |> Value.RawString

        | byt when
            format >= Format.NegativeFixInt ->

            byt
            |> intFromNegFixNum
            |> Value.Integer
        | byt ->
            let msg =
                sprintf "Unsupported format, header byte: %A" byt

            failwith msg

and readArrayValues
    (br: BufReader)
    (bytes: inref<Bytes>)
    (values: Stack<Value>)
    count
    =
    if count = 0 then
        Value.Array <| values.ToArray()
    else
        values.Push <| readNextValue br &bytes
        readArrayValues br &bytes values (count - 1)
and readMapValues
    (br: BufReader)
    (bytes: inref<Bytes>)
    (values: Dictionary<Value, Value>)
    count
    =
    if count = 0 then
        Value.Map values
    else
        let key = readNextValue br &bytes
        let value = readNextValue br &bytes
        values.[key] <- value

        readMapValues br &bytes values (count - 1)
