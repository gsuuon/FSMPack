module FSMPack.Read

open Microsoft.FSharp.Core

open FSMPack.Spec
open FSMPack.Utility.Byte

open System
open System.Buffers.Binary
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

    if bytes.Length - idx < count then
        failwith
            ("Expected more bytes to read, wanted " + string count
            + " but had " + string (bytes.Length - idx) )

    bytes.Slice(idx, count)

let readString (br: BufReader) (bytes: inref<Bytes>) len =
    // netstandard2.1 adds ReadOnlySpan api for Encoding.x.GetString
    (readBytes br &bytes len).ToArray()
    |> Text.Encoding.UTF8.GetString
    |> Value.RawString

let rec readValue (br: BufReader) (bytes: inref<Bytes>) =
    match Cast.asFormat <| readByte br &bytes with
    | Format.Nil -> Value.Nil
    | Format.False -> Value.Boolean false
    | Format.True -> Value.Boolean true
    | Format.UInt8 ->
        readByte br &bytes
        |> int
        |> Value.Integer
    | Format.UInt16 ->
        BinaryPrimitives.ReadUInt16BigEndian
            // Can't pipe into byref function
            // https://github.com/dotnet/fsharp/issues/5286#issuecomment-402249997
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
            (readBytes br &bytes 4)
        |> int
        |> Value.Integer
    | Format.Int64 ->
        BinaryPrimitives.ReadInt64BigEndian
            (readBytes br &bytes 8)
        |> Value.Integer64
    | Format.Float32 ->
        let floatBytes = (readBytes br &bytes 4).ToArray()

        if BitConverter.IsLittleEndian then
            Array.Reverse floatBytes

        BitConverter.ToSingle (floatBytes, 0)
        |> float
        |> Value.Float
    | Format.Float64 ->
        let floatBytes = (readBytes br &bytes 8).ToArray()

        if BitConverter.IsLittleEndian then
            Array.Reverse floatBytes

        BitConverter.ToDouble (floatBytes, 0)
        |> Value.Float
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
            let u32 =
                BinaryPrimitives.ReadUInt32BigEndian
                    (readBytes br &bytes 4)
            if u32 = UInt32.MaxValue then raise <| OverflowException()

            int u32

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
        match Cast.asValue format with
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
                maskByte 0b11110000uy byt
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

            readString br &bytes len

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
    let mutable curCount = count

    while count <> 0 do
        values.Push <| readValue br &bytes
        curCount <- curCount - 1

    Value.Array <| values.ToArray()
and readMapValues
    (br: BufReader)
    (bytes: inref<Bytes>)
    (values: Dictionary<Value, Value>)
    count
    =
    let mutable curCount = count

    while count <> 0 do
        let key = readValue br &bytes
        let value = readValue br &bytes
        values.[key] <- value
        curCount <- curCount - 1

    Value.Map values
