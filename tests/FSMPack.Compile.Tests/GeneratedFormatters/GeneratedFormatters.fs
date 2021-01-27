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


type FormatMyTestType() =
    interface Format<MyTestType> with
        member _.Write bw (v: MyTestType) =
            writeMapFormat bw 3
            writeValue bw (RawString "A")
            writeValue bw (Integer v.A)
            writeValue bw (RawString "B")
            writeValue bw (FloatDouble v.B)
            writeValue bw (RawString "inner")
            Cache<MyInnerType>.Retrieve().Write bw v.inner

        member _.Read (br, bytes) =
            let count = 3
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable A = Unchecked.defaultof<Int32>
            let mutable B = Unchecked.defaultof<Double>
            let mutable inner = Unchecked.defaultof<MyInnerType>
            while items < count do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "A" ->
                        let (Integer x) = readValue br &bytes
                        A <- x
                    | "B" ->
                        let (FloatDouble x) = readValue br &bytes
                        B <- x
                    | "inner" ->
                        inner <- Cache<MyInnerType>.Retrieve().Read(br, bytes)
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                A = A
                B = B
                inner = inner
            }
