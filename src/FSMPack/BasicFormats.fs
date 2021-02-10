module FSMPack.BasicFormats

open System.Collections.Generic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write
open FSMPack.Format

#nowarn "0025"

type FormatIDictionary<'K, 'V when 'K : comparison>() =
    member _.WriteBase (bw, v: IDictionary<'K, 'V>) =
        let keyFormat = Cache<'K>.Retrieve()
        let valueFormat = Cache<'V>.Retrieve()

        writeMapFormat bw v.Count

        Seq.iter
            (fun (kv: KeyValuePair<'K, 'V>) ->
                keyFormat.Write bw kv.Key
                valueFormat.Write bw kv.Value )
            v

    member _.ReadBase (br, bytes) =
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

        collection :> IDictionary<'K, 'V>

    interface FSMPack.Format.Format<IDictionary<'K, 'V>> with
    // TODO if 'K and 'V are msgpack types, we could use Value.MapCollection
        member this.Write bw (v: IDictionary<'K, 'V>) =
            this.WriteBase (bw, v)

        member this.Read (br, bytes) =
            this.ReadBase (br, bytes)

type FormatMap<'K, 'V when 'K : comparison>() =
    inherit FormatIDictionary<'K, 'V>()
    interface Format<Map<'K, 'V>> with
        // FIXME FS0419 can't use curried members of base class
        // Need to change interface Format.Write to (bw, v)
        // but I think there were issues with that due to byref
        // Generally should avoid curried methods for C# interop
        member _.Write bw (v: Map<'K, 'V>) =
            base.WriteBase (bw, v)

        member _.Read (br, bytes) =
            base.ReadBase (br, bytes)
            |> Seq.map (|KeyValue|)
            |> Map.ofSeq

type FormatDictionary<'K, 'V when 'K : comparison>() =
    inherit FormatIDictionary<'K, 'V>()
    interface Format<Dictionary<'K, 'V>> with
        member _.Write bw v =
            base.WriteBase (bw, v)

        member _.Read (br, bytes) =
            base.ReadBase (br, bytes)
            |> Dictionary
            

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
    
    Cache<IDictionary<_,_>>.StoreGeneric typedefof<FormatIDictionary<_,_>>
    Cache<Dictionary<_,_>>.StoreGeneric typedefof<FormatDictionary<_,_>>
    Cache<Map<_,_>>.StoreGeneric typedefof<FormatMap<_,_>>
