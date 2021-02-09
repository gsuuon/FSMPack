module FSMPack.BasicFormats

open System.Collections.Generic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write
open FSMPack.Format

#nowarn "0025"

type FormatMap<'K, 'V when 'K : comparison>() =
    interface FSMPack.Format.Format<Map<'K, 'V>> with
    // TODO if 'K and 'V are msgpack types, we could use Value.MapCollection
        member _.Write bw (v: Map<'K, 'V>) =
            let keyFormat = Cache<'K>.Retrieve()
            let valueFormat = Cache<'V>.Retrieve()

            writeMapFormat bw v.Count

            Map.iter
                (fun k x ->
                    keyFormat.Write bw k
                    valueFormat.Write bw x)
                v

        member _.Read (br, bytes) =
            let keyFormat = Cache<'K>.Retrieve()
            let valueFormat = Cache<'V>.Retrieve()

            let expectedCount = readMapFormatCount br &bytes

            let mutable items = 0
            let collection = Dictionary()

            while items < expectedCount do
                let key = keyFormat.Read (br, bytes)
                let value = valueFormat.Read (br, bytes)
                collection.[key] <- value
                items <- items + 1

            collection
            |> Seq.map (|KeyValue|)
            |> Map.ofSeq

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
    
    Cache<Map<_,_>>.StoreGeneric typedefof<FormatMap<_,_>>
