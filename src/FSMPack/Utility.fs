module FSMPack.Utility

open System
open System.Buffers.Binary


module Byte =
    type Bytes = System.ReadOnlySpan<byte>

    let intFromBytes (bytes: Bytes) =
        BinaryPrimitives.ReadInt32BigEndian bytes

    /// 1's become 0's - mask is bits to set to 0
    let maskByte mask (byt: byte) =
        byt
        |> (^^^) mask
        |> (&&&) byt
