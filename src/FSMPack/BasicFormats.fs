module FSMPack.BasicFormats

open System.Collections.Generic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write
open FSMPack.Format

#nowarn "0025"

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
            
type FormatOption<'T>() =
    interface Format<'T option> with
        member _.Write bw v =
            match v with
            | Some x ->
                Cache<'T>.Retrieve().Write bw x
            | None ->
                writeValue bw Nil
        member _.Read (br, bytes) =
            match Cast.asFormat bytes.[br.idx] with
            | Format.Nil ->
                br.Advance 1
                None
            | _ ->
                Cache<'T>.Retrieve().Read (br, bytes)
                |> Some

let writeSeq bw (v: 'T seq) len =
    writeValue bw (Integer len)
    
    let format = Cache<'T>.Retrieve()

    Seq.iter
        (fun x -> format.Write bw x)
        v

let readSeq (br, bytes) =
    let (Integer expectedCount) = readValue br &bytes

    let mutable count = 0
    let items = Queue()

    let format = Cache<'T>.Retrieve()

    while count < expectedCount do
        items.Enqueue <| format.Read (br, bytes)
        count <- count + 1

    items
    
type FormatFSharpList<'T>() =
    interface Format<'T list> with
        member _.Write bw v =
            writeSeq bw v v.Length

        member _.Read (br, bytes) =
            readSeq (br, bytes)
            |> Seq.toList

type FormatFSharpArray<'T>() =
    interface Format<'T array> with
        member _.Write bw v =
            writeSeq bw v v.Length

        member _.Read (br, bytes) =
            readSeq (br, bytes)
            |> Seq.toArray

let setup () =
    // TODO should probably just generate these as well
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
    Cache<IDictionary<_,_>>.StoreGeneric typedefof<FormatIDictionary<_,_>>
    Cache<Dictionary<_,_>>.StoreGeneric typedefof<FormatDictionary<_,_>>
    Cache<Map<_,_>>.StoreGeneric typedefof<FormatMap<_,_>>
    Cache<_ option>.StoreGeneric typedefof<FormatOption<_>>
    Cache<_ list>.StoreGeneric typedefof<FormatFSharpList<_>>
    Cache<_ array>.StoreGeneric typedefof<FormatFSharpArray<_>>
