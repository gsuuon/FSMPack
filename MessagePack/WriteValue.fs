module FSMPack.WriteValue

open System
open System.Buffers
open System.Buffers.Binary
open System.Runtime

open FSMPack.Spec

type BufWriter =
    {
        mutable idx : int
        mutable buffer : byte[] // necessary for GetSpan/GetMemory
        initialSize : int
    }
    member private x.SetSize size =
        Array.Resize(ref x.buffer, size)

    member private x.Resize sizeHint =
            // TODO linked list of arrays rather than resizing
        if x.buffer.Length = 0 then
            x.SetSize x.initialSize
        else if sizeHint < x.buffer.Length then
            x.SetSize (x.buffer.Length + sizeHint)
        else
            x.SetSize (x.buffer.Length * 2)

    member private x.CheckResize sizeHint =
        if x.idx + sizeHint >= x.buffer.Length
        then x.Resize sizeHint

    member x.GetSpan sizeHint =
        x.CheckResize sizeHint
        x.buffer.AsSpan(x.idx)

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

/// Positive or negative integer of 4 bytes max
let writeInteger (bw: BufWriter) i =
    if i >= 0 then
        if i < 128 then
            Cast.asValue Format.PositiveFixInt &&& byte i 
            |> writeByte bw
        else if i <= 32767 then
            writeByte bw (Cast.asValue Format.Int16)
            BinaryPrimitives.WriteInt16BigEndian
                (bw.GetSpan 2, int16 i)
        else if i <= 2147483647 then
            writeByte bw (Cast.asValue Format.Int32)
            BinaryPrimitives.WriteInt32BigEndian
                (bw.GetSpan 4, i)
        else
            failwith WriteSizeIntError
    else
        if i > -32 then
            Cast.asValue Format.NegativeFixInt &&& byte i 
            |> writeByte bw
        else if i >= -128 then
            writeByte bw (Cast.asValue Format.Int8)
            writeByte bw (byte (sbyte i))
        else if i >= -32768 then
            writeByte bw (Cast.asValue Format.Int16)
            BinaryPrimitives.WriteInt16BigEndian
                (bw.GetSpan 2, int16 i)
        else if i >= -2147483648 then
            writeByte bw (Cast.asValue Format.Int32)
            BinaryPrimitives.WriteInt32BigEndian
                (bw.GetSpan 4, i)
        else
            failwith WriteSizeIntError

let writeUInteger (bw: BufWriter) (ui: uint32) =
    if ui <= 255u then
        writeByte bw (Cast.asValue Format.UInt8)
        writeByte bw (byte ui)
    else if ui <= 32767u then
        writeByte bw (Cast.asValue Format.UInt16)
        BinaryPrimitives.WriteUInt16BigEndian
            (bw.GetSpan 2, uint16 ui)
    else if ui <= 2147483647u then
        writeByte bw (Cast.asValue Format.UInt32)
        BinaryPrimitives.WriteUInt32BigEndian
            (bw.GetSpan 4, ui)
    else
        failwith WriteSizeIntError

let writeValue (bw: BufWriter) mpv =
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
        BinaryPrimitives.WriteInt64BigEndian
            (bw.GetSpan 8, x)
    | UInteger x ->
        writeUInteger bw x
    | UInteger64 x ->
        writeByte bw (Cast.asValue Format.UInt64)
        BinaryPrimitives.WriteUInt64BigEndian
            (bw.GetSpan 8, x)
    | _ ->
        ()
