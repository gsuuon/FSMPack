module FSMPack.GeneratedFormatters

open System

open FSMPack.Format
open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

#nowarn "0025"


open FSMPack.Tests.Types.Record
open FSMPack.Tests.Types.DU
open FSMPack.Tests.Types.Mixed

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
            | MyInnerDU.A ->
                writeArrayFormat bw 1
                writeValue bw (Integer 0)
            | MyInnerDU.B (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                writeValue bw (Integer x0)

        member _.Read (br, bytes) =
            let count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                MyInnerDU.A
            | Integer 1 ->
                let (Integer x0) = readValue br &bytes
                MyInnerDU.B (x0)
            | _ ->
                failwith "Unexpected DU case tag"

type FormatMyDU() =
    interface Format<MyDU> with
        member _.Write bw (v: MyDU) =
            match v with
            | MyDU.C (x0, x1) ->
                writeArrayFormat bw 3
                writeValue bw (Integer 0)
                writeValue bw (RawString x0)
                writeValue bw (FloatDouble x1)
            | MyDU.D (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                Cache<FSMPack.Tests.Types.DU.MyInnerDU>.Retrieve().Write bw x0
            | MyDU.E ->
                writeArrayFormat bw 1
                writeValue bw (Integer 2)

        member _.Read (br, bytes) =
            let count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                let (RawString x0) = readValue br &bytes
                let (FloatDouble x1) = readValue br &bytes
                MyDU.C (x0, x1)
            | Integer 1 ->
                let x0 = Cache<FSMPack.Tests.Types.DU.MyInnerDU>.Retrieve().Read(br, bytes)
                MyDU.D (x0)
            | Integer 2 ->
                MyDU.E
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

type FormatFoo() =
    interface Format<Foo> with
        member _.Write bw (v: Foo) =
            writeMapFormat bw 1
            writeValue bw (RawString "a")
            writeValue bw (Integer v.a)

        member _.Read (br, bytes) =
            let count = 1
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable a = Unchecked.defaultof<Int32>
            while items < count do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "a" ->
                        let (Integer x) = readValue br &bytes
                        a <- x
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                a = a
            }

type FormatBar() =
    interface Format<Bar> with
        member _.Write bw (v: Bar) =
            match v with
            | Bar.A (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 0)
                Cache<FSMPack.Tests.Types.Mixed.Foo>.Retrieve().Write bw x0
            | Bar.B (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                writeValue bw (FloatDouble x0)

        member _.Read (br, bytes) =
            let count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                let x0 = Cache<FSMPack.Tests.Types.Mixed.Foo>.Retrieve().Read(br, bytes)
                Bar.A (x0)
            | Integer 1 ->
                let (FloatDouble x0) = readValue br &bytes
                Bar.B (x0)
            | _ ->
                failwith "Unexpected DU case tag"

type FormatBaz<'T>() =
    interface Format<Baz<'T>> with
        member _.Write bw (v: Baz<'T>) =
            writeMapFormat bw 3
            writeValue bw (RawString "b")
            writeValue bw (RawString v.b)
            writeValue bw (RawString "bar")
            Cache<Bar>.Retrieve().Write bw v.bar
            writeValue bw (RawString "c")
            Cache<'T>.Retrieve().Write bw v.c

        member _.Read (br, bytes) =
            let count = 3
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable b = Unchecked.defaultof<String>
            let mutable bar = Unchecked.defaultof<Bar>
            let mutable c = Unchecked.defaultof<'T>
            while items < count do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "b" ->
                        let (RawString x) = readValue br &bytes
                        b <- x
                    | "bar" ->
                        bar <- Cache<Bar>.Retrieve().Read(br, bytes)
                    | "c" ->
                        c <- Cache<'T>.Retrieve().Read(br, bytes)
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                b = b
                bar = bar
                c = c
            }
