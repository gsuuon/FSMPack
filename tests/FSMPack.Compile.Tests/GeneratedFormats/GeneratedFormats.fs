module FSMPack.GeneratedFormats

open System

open FSMPack.Format
open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

#nowarn "0025"

let mutable _initStartupCode = 0


type FMT_FSMPack_Tests_Types_Collection_FSharpCollectionContainer() =
    interface Format<FSMPack.Tests.Types.Collection.FSharpCollectionContainer> with
        member _.Write bw (v: FSMPack.Tests.Types.Collection.FSharpCollectionContainer) =
            writeMapFormat bw 1
            writeValue bw (RawString "myMap")
            Cache<Map<_,_>>.Retrieve().Write bw v.myMap

        member _.Read (br, bytes) =
            let count = 1
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable myMap = Unchecked.defaultof<Map<_,_>>
            while items < count do
                match readValue br &bytes with
                | RawString _k ->
                    match _k with
                    | "myMap" ->
                        myMap <- Cache<Map<_,_>>.Retrieve().Read(br, bytes)
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                myMap = myMap
            }

Cache<FSMPack.Tests.Types.Collection.FSharpCollectionContainer>.Store (FMT_FSMPack_Tests_Types_Collection_FSharpCollectionContainer() :> Format<FSMPack.Tests.Types.Collection.FSharpCollectionContainer>)

type FMT_FSMPack_Tests_Types_Mixed_Baz<'T>() =
    interface Format<FSMPack.Tests.Types.Mixed.Baz<'T>> with
        member _.Write bw (v: FSMPack.Tests.Types.Mixed.Baz<'T>) =
            writeMapFormat bw 3
            writeValue bw (RawString "b")
            writeValue bw (RawString v.b)
            writeValue bw (RawString "bar")
            Cache<FSMPack.Tests.Types.Mixed.Bar>.Retrieve().Write bw v.bar
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
            let mutable b = Unchecked.defaultof<System.String>
            let mutable bar = Unchecked.defaultof<FSMPack.Tests.Types.Mixed.Bar>
            let mutable c = Unchecked.defaultof<'T>
            while items < count do
                match readValue br &bytes with
                | RawString _k ->
                    match _k with
                    | "b" ->
                        let (RawString x) = readValue br &bytes
                        b <- x
                    | "bar" ->
                        bar <- Cache<FSMPack.Tests.Types.Mixed.Bar>.Retrieve().Read(br, bytes)
                    | "c" ->
                        c <- Cache<'T>.Retrieve().Read(br, bytes)
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                b = b
                bar = bar
                c = c
            }

Cache<FSMPack.Tests.Types.Mixed.Baz<_>>.StoreGeneric typedefof<FMT_FSMPack_Tests_Types_Mixed_Baz<_>>

type FMT_FSMPack_Tests_Types_Mixed_Foo() =
    interface Format<FSMPack.Tests.Types.Mixed.Foo> with
        member _.Write bw (v: FSMPack.Tests.Types.Mixed.Foo) =
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
            let mutable a = Unchecked.defaultof<System.Int32>
            while items < count do
                match readValue br &bytes with
                | RawString _k ->
                    match _k with
                    | "a" ->
                        let (Integer x) = readValue br &bytes
                        a <- x
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                a = a
            }

Cache<FSMPack.Tests.Types.Mixed.Foo>.Store (FMT_FSMPack_Tests_Types_Mixed_Foo() :> Format<FSMPack.Tests.Types.Mixed.Foo>)

type FMT_FSMPack_Tests_Types_Record_MyGenericRecord<'T>() =
    interface Format<FSMPack.Tests.Types.Record.MyGenericRecord<'T>> with
        member _.Write bw (v: FSMPack.Tests.Types.Record.MyGenericRecord<'T>) =
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
                | RawString _k ->
                    match _k with
                    | "foo" ->
                        foo <- Cache<'T>.Retrieve().Read(br, bytes)
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                foo = foo
            }

Cache<FSMPack.Tests.Types.Record.MyGenericRecord<_>>.StoreGeneric typedefof<FMT_FSMPack_Tests_Types_Record_MyGenericRecord<_>>

type FMT_FSMPack_Tests_Types_Record_MyTestType() =
    interface Format<FSMPack.Tests.Types.Record.MyTestType> with
        member _.Write bw (v: FSMPack.Tests.Types.Record.MyTestType) =
            writeMapFormat bw 3
            writeValue bw (RawString "A")
            writeValue bw (Integer v.A)
            writeValue bw (RawString "B")
            writeValue bw (FloatDouble v.B)
            writeValue bw (RawString "inner")
            Cache<FSMPack.Tests.Types.Record.MyInnerType>.Retrieve().Write bw v.inner

        member _.Read (br, bytes) =
            let count = 3
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable A = Unchecked.defaultof<System.Int32>
            let mutable B = Unchecked.defaultof<System.Double>
            let mutable inner = Unchecked.defaultof<FSMPack.Tests.Types.Record.MyInnerType>
            while items < count do
                match readValue br &bytes with
                | RawString _k ->
                    match _k with
                    | "A" ->
                        let (Integer x) = readValue br &bytes
                        A <- x
                    | "B" ->
                        let (FloatDouble x) = readValue br &bytes
                        B <- x
                    | "inner" ->
                        inner <- Cache<FSMPack.Tests.Types.Record.MyInnerType>.Retrieve().Read(br, bytes)
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                A = A
                B = B
                inner = inner
            }

Cache<FSMPack.Tests.Types.Record.MyTestType>.Store (FMT_FSMPack_Tests_Types_Record_MyTestType() :> Format<FSMPack.Tests.Types.Record.MyTestType>)

type FMT_FSMPack_Tests_Types_Record_MyInnerType() =
    interface Format<FSMPack.Tests.Types.Record.MyInnerType> with
        member _.Write bw (v: FSMPack.Tests.Types.Record.MyInnerType) =
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
            let mutable C = Unchecked.defaultof<System.String>
            while items < count do
                match readValue br &bytes with
                | RawString _k ->
                    match _k with
                    | "C" ->
                        let (RawString x) = readValue br &bytes
                        C <- x
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                C = C
            }

Cache<FSMPack.Tests.Types.Record.MyInnerType>.Store (FMT_FSMPack_Tests_Types_Record_MyInnerType() :> Format<FSMPack.Tests.Types.Record.MyInnerType>)

type FMT_FSMPack_Tests_Types_DU_MyGenDU<'T>() =
    interface Format<FSMPack.Tests.Types.DU.MyGenDU<'T>> with
        member _.Write bw (v: FSMPack.Tests.Types.DU.MyGenDU<'T>) =
            match v with
            | FSMPack.Tests.Types.DU.MyGenDU.MyT (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 0)
                Cache<'T>.Retrieve().Write bw x0
            | FSMPack.Tests.Types.DU.MyGenDU.Foo ->
                writeArrayFormat bw 1
                writeValue bw (Integer 1)

        member _.Read (br, bytes) =
            let _count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                let x0 = Cache<'T>.Retrieve().Read(br, bytes)
                FSMPack.Tests.Types.DU.MyGenDU<'T>.MyT (x0)
            | Integer 1 ->
                FSMPack.Tests.Types.DU.MyGenDU<'T>.Foo
            | _ ->
                failwith "Unexpected DU case tag"

Cache<FSMPack.Tests.Types.DU.MyGenDU<_>>.StoreGeneric typedefof<FMT_FSMPack_Tests_Types_DU_MyGenDU<_>>

type FMT_FSMPack_Tests_Types_Mixed_Bar() =
    interface Format<FSMPack.Tests.Types.Mixed.Bar> with
        member _.Write bw (v: FSMPack.Tests.Types.Mixed.Bar) =
            match v with
            | FSMPack.Tests.Types.Mixed.Bar.A (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 0)
                Cache<FSMPack.Tests.Types.Mixed.Foo>.Retrieve().Write bw x0
            | FSMPack.Tests.Types.Mixed.Bar.B (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                writeValue bw (FloatDouble x0)

        member _.Read (br, bytes) =
            let _count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                let x0 = Cache<FSMPack.Tests.Types.Mixed.Foo>.Retrieve().Read(br, bytes)
                FSMPack.Tests.Types.Mixed.Bar.A (x0)
            | Integer 1 ->
                let (FloatDouble x0) = readValue br &bytes
                FSMPack.Tests.Types.Mixed.Bar.B (x0)
            | _ ->
                failwith "Unexpected DU case tag"

Cache<FSMPack.Tests.Types.Mixed.Bar>.Store (FMT_FSMPack_Tests_Types_Mixed_Bar() :> Format<FSMPack.Tests.Types.Mixed.Bar>)

type FMT_FSMPack_Tests_Types_DU_MyDU() =
    interface Format<FSMPack.Tests.Types.DU.MyDU> with
        member _.Write bw (v: FSMPack.Tests.Types.DU.MyDU) =
            match v with
            | FSMPack.Tests.Types.DU.MyDU.C (x0, x1) ->
                writeArrayFormat bw 3
                writeValue bw (Integer 0)
                writeValue bw (RawString x0)
                writeValue bw (FloatDouble x1)
            | FSMPack.Tests.Types.DU.MyDU.D (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                Cache<FSMPack.Tests.Types.DU.MyInnerDU>.Retrieve().Write bw x0
            | FSMPack.Tests.Types.DU.MyDU.E ->
                writeArrayFormat bw 1
                writeValue bw (Integer 2)

        member _.Read (br, bytes) =
            let _count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                let (RawString x0) = readValue br &bytes
                let (FloatDouble x1) = readValue br &bytes
                FSMPack.Tests.Types.DU.MyDU.C (x0, x1)
            | Integer 1 ->
                let x0 = Cache<FSMPack.Tests.Types.DU.MyInnerDU>.Retrieve().Read(br, bytes)
                FSMPack.Tests.Types.DU.MyDU.D (x0)
            | Integer 2 ->
                FSMPack.Tests.Types.DU.MyDU.E
            | _ ->
                failwith "Unexpected DU case tag"

Cache<FSMPack.Tests.Types.DU.MyDU>.Store (FMT_FSMPack_Tests_Types_DU_MyDU() :> Format<FSMPack.Tests.Types.DU.MyDU>)

type FMT_FSMPack_Tests_Types_DU_MyInnerDU() =
    interface Format<FSMPack.Tests.Types.DU.MyInnerDU> with
        member _.Write bw (v: FSMPack.Tests.Types.DU.MyInnerDU) =
            match v with
            | FSMPack.Tests.Types.DU.MyInnerDU.A ->
                writeArrayFormat bw 1
                writeValue bw (Integer 0)
            | FSMPack.Tests.Types.DU.MyInnerDU.B (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                writeValue bw (Integer x0)

        member _.Read (br, bytes) =
            let _count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                FSMPack.Tests.Types.DU.MyInnerDU.A
            | Integer 1 ->
                let (Integer x0) = readValue br &bytes
                FSMPack.Tests.Types.DU.MyInnerDU.B (x0)
            | _ ->
                failwith "Unexpected DU case tag"

Cache<FSMPack.Tests.Types.DU.MyInnerDU>.Store (FMT_FSMPack_Tests_Types_DU_MyInnerDU() :> Format<FSMPack.Tests.Types.DU.MyInnerDU>)

let initialize () =
    FSMPack.BasicFormats.setup ()

    _initStartupCode
