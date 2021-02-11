module FSMPack.Formats.Standard.IDict

open System.Collections.Generic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write
open FSMPack.Format

let writeIDict (bw, v: IDictionary<'K, 'V>) =
    let keyFormat = Cache<'K>.Retrieve()
    let valueFormat = Cache<'V>.Retrieve()

    writeMapFormat bw v.Count

    Seq.iter
        (fun (kv: KeyValuePair<'K, 'V>) ->
            keyFormat.Write bw kv.Key
            valueFormat.Write bw kv.Value )
        v

let readIDict (br, bytes) =
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

// TODO if 'K and 'V are msgpack types, we could use Value.MapCollection
type FormatIDictionary<'K, 'V when 'K : comparison>() =
    interface FSMPack.Format.Format<IDictionary<'K, 'V>> with
        member _.Write bw (v: IDictionary<'K, 'V>) =
            writeIDict (bw, v)

        member _.Read (br, bytes) =
            readIDict (br, bytes)

type FormatMap<'K, 'V when 'K : comparison>() =
    interface Format<Map<'K, 'V>> with
        member _.Write bw (v: Map<'K, 'V>) =
            writeIDict (bw, v)

        member _.Read (br, bytes) =
            readIDict (br, bytes)
            |> Seq.map (|KeyValue|)
            |> Map.ofSeq

type FormatDictionary<'K, 'V when 'K : comparison>() =
    interface Format<Dictionary<'K, 'V>> with
        member _.Write bw v =
            writeIDict (bw, v)

        member _.Read (br, bytes) =
            readIDict (br, bytes)
            |> Dictionary
            
