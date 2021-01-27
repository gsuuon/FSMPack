module FSMPack.GeneratedFormatters

open System

open FSMPack.Format
open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

#nowarn "0025"


open FSMPack.Tests.Types.Record
open FSMPack.Tests.Types.DU

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

type FormatMyInnerDU() =
    interface Format<MyInnerDU> with
        member _.Write bw (v: MyInnerDU) =
            match v with
            | A ->
                writeArrayFormat bw 1
                writeValue bw (Integer 0)
            | B (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                writeValue bw (Integer x0)

        member _.Read (br, bytes) =
            let count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                A
            | Integer 1 ->
                let (Integer x0) = readValue br &bytes
                B (x0)
            | _ ->
                failwith "Unexpected DU case tag"

type FormatMyDU() =
    interface Format<MyDU> with
        member _.Write bw (v: MyDU) =
            match v with
            | C (x0, x1) ->
                writeArrayFormat bw 3
                writeValue bw (Integer 0)
                writeValue bw (RawString x0)
                writeValue bw (FloatDouble x1)
            | D (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                Cache<FSMPack.Tests.Types.DU.MyInnerDU>.Retrieve().Write bw x0
            | E ->
                writeArrayFormat bw 1
                writeValue bw (Integer 2)

        member _.Read (br, bytes) =
            let count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                let (RawString x0) = readValue br &bytes
                let (FloatDouble x1) = readValue br &bytes
                C (x0, x1)
            | Integer 1 ->
                let x0 = Cache<FSMPack.Tests.Types.DU.MyInnerDU>.Retrieve().Read(br, bytes)
                D (x0)
            | Integer 2 ->
                E
            | _ ->
                failwith "Unexpected DU case tag"

type FormatMyGenericRecord<'T>() =
    interface Format<MyGenericRecord<'T>> with
        member _.Write bw (v: MyGenericRecord<'T>) =
            writeMapFormat bw 1
            writeValue bw (RawString "foo")
            Cache<'T>.Retrieve().Write bw v.foo

        member _.Read (br, bytes) =
            let count = 1
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable foo = Unchecked.defaultof<'T>
            while items < count do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "foo" ->
                        foo <- Cache<'T>.Retrieve().Read(br, bytes)
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                foo = foo
            }
