module FSMPack.Formats.Standard.Sequence

open System.Collections.Generic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write
open FSMPack.Format

#nowarn "0025"

let writeSeq bw (v: 'T seq) len =
    if len > 0 then
        writeValue bw (Integer len)

        let format = Cache<'T>.Retrieve()

        Seq.iter
            (fun x -> format.Write bw x)
            v
    else
        writeValue bw (Integer 0)

let readSeq (br, bytes) =
    let (Integer expectedCount) = readValue br &bytes

    let mutable count = 0
    let items = Queue()

    if expectedCount > 0 then
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
