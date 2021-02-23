module FSMPack.Formats.Standard.Primitives

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write
open FSMPack.Format

#nowarn "0025"

type FormatInt() =
    interface Format<int> with
        member _.Write bw (v: int) =
            writeValue bw (Integer v)
        member _.Read (br, bytes) =
            let (Integer x) = readValue br &bytes
            x

type FormatInt16() =
    interface Format<int16> with
        member _.Write bw (v: int16) =
            writeValue bw (Integer (int v))
        member _.Read (br, bytes) =
            let (Integer x) = readValue br &bytes
            int16 x

type FormatInt64() =
    interface Format<int64> with
        member _.Write bw (v: int64) =
            writeValue bw (Integer64 v)
        member _.Read (br, bytes) =
            let (Integer64 x) = readValue br &bytes
            x

type FormatFloat() =
    interface Format<float> with
        member _.Write bw (v: float) =
            writeValue bw (FloatDouble v)
        member _.Read (br, bytes) =
            let (FloatDouble x) = readValue br &bytes
            x 

type FormatBool() =
    interface Format<bool> with
        member _.Write bw (v: bool) =
            writeValue bw (Boolean v)
        member _.Read (br, bytes) =
            let (Boolean x) = readValue br &bytes
            x 

type FormatByte() =
    interface Format<byte> with
        member _.Write bw (v: byte) =
            writeByte bw v
        member _.Read (br, bytes) =
            readByte br &bytes

type FormatString() = // Not actually a primitive but..
    interface Format<string> with
        member _.Write bw (v: string) =
            writeValue bw (RawString v)
        member _.Read (br, bytes) =
            let (RawString x) = readValue br &bytes
            x
