module FSMPack.Formats.Default

open System.Collections.Generic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write
open FSMPack.Format

open FSMPack.Formats.Standard

#nowarn "0025"

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

let setup () =
    FSMPack.Formats.Basic.setup()

    Cache<IDictionary<_,_>>.StoreGeneric typedefof<IDict.FormatIDictionary<_,_>>
    Cache<Dictionary<_,_>>.StoreGeneric typedefof<IDict.FormatDictionary<_,_>>
    Cache<Map<_,_>>.StoreGeneric typedefof<IDict.FormatMap<_,_>>

    Cache<System.Tuple<_,_>>.StoreGeneric typedefof<Tuples.FormatFSharpTuple2<_,_>>
    Cache<System.Tuple<_,_,_>>.StoreGeneric typedefof<Tuples.FormatFSharpTuple3<_,_,_>>

    Cache<_ option>.StoreGeneric typedefof<FormatOption<_>>
    Cache<_ list>.StoreGeneric typedefof<Sequence.FormatFSharpList<_>>
    Cache<_ array>.StoreGeneric typedefof<Sequence.FormatFSharpArray<_>>
