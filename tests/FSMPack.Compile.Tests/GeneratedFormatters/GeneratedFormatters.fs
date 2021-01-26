module FSMPack.GeneratedFormatters

open System

open FSMPack.Format
open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

open FSMPack.Tests.Types.Record

#nowarn "0025"

type FormatMyInnerType() =
    interface Format<MyInnerType> with
        member _.Write bw (v: MyInnerType) =
            writeMapFormat bw 1
            writeValue bw (RawString "C")
            writeValue bw (RawString v.C)

        member _.Read (br, bytes) =
            let count = 1
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable C = Unchecked.defaultof<String>
            while items < count do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "C" ->
                        let (RawString x) = readValue br &bytes
                        C <- x
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                C = C
            }
