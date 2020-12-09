module FSMPack.WritePrimitive

open System.Buffers
open System.Buffers.Binary

type BytesWriter = IBufferWriter<byte>

let writeUInt16 (bw: BytesWriter) (ui: uint32) =
    BinaryPrimitives.WriteUInt16BigEndian
        (bw.GetSpan 2, uint16 ui)

    bw.Advance 2

let writeUInt32 (bw: BytesWriter) (ui: uint32) =
    BinaryPrimitives.WriteUInt32BigEndian
        (bw.GetSpan 4, ui)

    bw.Advance 4

let writeUInt64 (bw: BytesWriter) i =
    BinaryPrimitives.WriteUInt64BigEndian
        (bw.GetSpan 8, i)

    bw.Advance 8

let writeInt16 (bw: BytesWriter) i =
    BinaryPrimitives.WriteInt16BigEndian
        (bw.GetSpan 2, int16 i)

    bw.Advance 2

let writeInt32 (bw: BytesWriter) i =
    BinaryPrimitives.WriteInt32BigEndian
        (bw.GetSpan 4, i)

    bw.Advance 4

let writeInt64 (bw: BytesWriter) i =
    BinaryPrimitives.WriteInt64BigEndian
        (bw.GetSpan 8, i)

    bw.Advance 8
