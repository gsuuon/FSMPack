module FSMPack.Formats.Basic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write
open FSMPack.Format

#nowarn "0025"

// TODO should probably just generate these as well
let setup () =
    Cache<string>.Store
      { new Format<string> with
        member _.Write bw (v: string) =
            writeValue bw (RawString v)
        member _.Read (br, bytes) =
            let (RawString x) = readValue br &bytes
            x } 

    Cache<int>.Store
      { new Format<int> with
        member _.Write bw (v: int) =
            writeValue bw (Integer v)
        member _.Read (br, bytes) =
            let (Integer x) = readValue br &bytes
            x } 

    Cache<int16>.Store
      { new Format<int16> with
        member _.Write bw (v: int16) =
            writeValue bw (Integer (int v))
        member _.Read (br, bytes) =
            let (Integer x) = readValue br &bytes
            int16 x } 

    Cache<float>.Store
      { new Format<float> with
        member _.Write bw (v: float) =
            writeValue bw (FloatDouble v)
        member _.Read (br, bytes) =
            let (FloatDouble x) = readValue br &bytes
            x } 

    Cache<bool>.Store
      { new Format<bool> with
        member _.Write bw (v: bool) =
            writeValue bw (Boolean v)
        member _.Read (br, bytes) =
            let (Boolean x) = readValue br &bytes
            x } 

    FSMPack.FormatUnitWorkaround.FormatUnit.StoreFormat()
