module FSMPack.Write

open System
open System.Buffers
open System.Runtime

open FSMPack.Spec
open FSMPack.WritePrimitive

type BufWriter =
    {
        mutable idx : int
        mutable buffer : byte[] // necessary for GetSpan/GetMemory
        initialSize : int
    }
    member private x.SetSize size =
        Array.Resize(&x.buffer, size)

    member private x.Resize sizeHint =
            // TODO linked list of arrays rather than resizing
        if x.buffer.Length = 0 then
            x.SetSize (max (max x.initialSize 2) sizeHint)
        else if sizeHint > x.buffer.Length then
            x.SetSize (x.buffer.Length + sizeHint)
        else
            x.SetSize (x.buffer.Length * 2)

    member private x.CheckResize sizeHint =
        if x.idx + sizeHint >= x.buffer.Length
        then x.Resize sizeHint

    member x.GetSpan sizeHint =
        x.CheckResize sizeHint
        x.buffer.AsSpan(x.idx)

    member x.GetWritten () =
        if x.idx = 0 then Array.empty
        else x.buffer.[0..x.idx-1]

    interface IBufferWriter<byte> with
        member x.Advance count =
            x.idx <- x.idx + count
        member x.GetSpan sizeHint =
            x.GetSpan sizeHint
        member x.GetMemory sizeHint =
            x.CheckResize sizeHint
            x.buffer.AsMemory(x.idx)


let writeByte (bw: BufWriter) (byt: byte) =
    bw.Write (ReadOnlySpan [|byt|])

let writeBytes (bw: BufWriter) (bytes: byte[]) =
    bw.Write (ReadOnlySpan bytes)

let writeFormat (bw: BufWriter) (format: Format) =
    Cast.asValue format
    |> writeByte bw

[<Literal>]
let WriteSizeIntError = "Write value was expected to be int size or smaller"
let WriteSizeIntElemsError = "Write value was expected to be int size elements or fewer"

/// Positive or negative integer of 4 bytes max
let writeInteger (bw: BufWriter) i =
    if i >= 0 then
        if i < 128 then
            Cast.asValue Format.PositiveFixInt ||| byte i 
            |> writeByte bw
        else if i <= 127 then
            writeByte bw (Cast.asValue Format.Int8)
            writeByte bw (byte i)
        else if i <= 32767 then
            writeByte bw (Cast.asValue Format.Int16)
            writeInt16 bw i
        else
            writeByte bw (Cast.asValue Format.Int32)
            writeInt32 bw i
    else
        if i > -32 then
            Cast.asValue Format.NegativeFixInt ||| byte (-i)
            |> writeByte bw
        else if i >= -128 then
            writeByte bw (Cast.asValue Format.Int8)
            writeByte bw (byte (sbyte i))
        else if i >= -32768 then
            writeByte bw (Cast.asValue Format.Int16)
            writeInt16 bw i
        else
            writeByte bw (Cast.asValue Format.Int32)
            writeInt32 bw i

let writeUInteger (bw: BufWriter) (ui: uint32) =
    if ui <= 255u then
        writeByte bw (Cast.asValue Format.UInt8)
        writeByte bw (byte ui)
    else if ui <= 32767u then
        writeByte bw (Cast.asValue Format.UInt16)
        writeUInt16 bw ui
    else
        writeByte bw (Cast.asValue Format.UInt32)
        writeUInt32 bw ui

let writeString (bw: BufWriter) (s: string) =
    if s.Length <= 31 then
        let header =
            Cast.asValue Format.FixStr ||| byte s.Length

        writeByte bw header
    else if s.Length <= 255 then
        writeByte bw (Cast.asValue Format.Str8)
        writeByte bw (byte s.Length)
    else if s.Length <= 32767 then
        writeByte bw (Cast.asValue Format.Str16)
        writeUInt16 bw (uint32 s.Length)
    else if s.Length <= 2147483647 then
        writeByte bw (Cast.asValue Format.Str32)
        writeUInt32 bw (uint32 s.Length)
    else
        failwith WriteSizeIntElemsError

    writeBytes bw (System.Text.Encoding.UTF8.GetBytes s)

let singleMax = float Single.MaxValue
let singleMin = float Single.MinValue

let rec writeValue (bw: BufWriter) mpv =
    match mpv with
    | Nil ->
        writeByte bw (Cast.asValue Format.Nil)
    | Boolean x ->
        writeByte bw
            (if x then
                Cast.asValue Format.True
            else
                Cast.asValue Format.False)
        
    | Integer x ->
        writeInteger bw x
    | Integer64 x ->
        writeByte bw (Cast.asValue Format.Int64)
        writeInt64 bw x
    | FloatSingle x ->
        writeByte bw (Cast.asValue Format.Float32)

        let bytes = BitConverter.GetBytes x

        if BitConverter.IsLittleEndian then
            Array.Reverse bytes

        writeBytes bw bytes
    | FloatDouble x ->
        writeByte bw (Cast.asValue Format.Float64)

        let bytes = BitConverter.GetBytes x

        if BitConverter.IsLittleEndian then
            Array.Reverse bytes

        writeBytes bw bytes
        
    | UInteger x ->
        writeUInteger bw x
    | UInteger64 x ->
        writeByte bw (Cast.asValue Format.UInt64)
        writeUInt64 bw x
    | RawString x ->
        writeString bw x
    | RawBinary x ->
        if x.Length <= 255 then
            writeByte bw (Cast.asValue Format.Bin8)
        else if x.Length <= 32767 then
            writeByte bw (Cast.asValue Format.Bin16)
            writeUInt16 bw (uint32 x.Length)
        else if x.Length <= 2147483647 then
            writeByte bw (Cast.asValue Format.Bin32)
            writeUInt32 bw (uint32 x.Length)
        else
            failwith WriteSizeIntElemsError

        writeBytes bw x
    | Array x ->
        let len = x.Length

        if len <= 15 then
            let header = Cast.asValue Format.FixArray ||| byte len
            writeByte bw header
        else if len <= 32767 then
            writeByte bw (Cast.asValue Format.Array16)
            writeUInt16 bw (uint32 len)
        else if len <= 2147483647 then
            writeByte bw (Cast.asValue Format.Array32)
            writeUInt32 bw (uint32 len)
        else
            failwith WriteSizeIntElemsError

        let mutable i = 0
        while i < len do
            writeValue bw x.[i]
            i <- i + 1
    | Map x ->
        let len = x.Count

        if len <= 15 then
            let header = Cast.asValue Format.FixMap ||| byte len
            writeByte bw header
        else if len <= 32767 then
            writeByte bw (Cast.asValue Format.Map16)
            writeUInt16 bw (uint32 len)
        else if len <= 2147483647 then
            writeByte bw (Cast.asValue Format.Map32)
            writeUInt32 bw (uint32 len)
        else
            failwith WriteSizeIntElemsError

        for KeyValue(key, value) in x do
            writeValue bw key
            writeValue bw value
    | x ->
        let msg = "Tried to write unsupported value: " + x.ToString()

        failwith msg