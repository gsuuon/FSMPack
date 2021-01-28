module FSMPack.BasicFormatters

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write
open FSMPack.Format

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

    Cache<float>.Store
      { new Format<float> with
        member _.Write bw (v: float) =
            writeValue bw (FloatDouble v)
        member _.Read (br, bytes) =
            let (FloatDouble x) = readValue br &bytes
            x } 
