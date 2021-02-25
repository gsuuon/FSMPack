module FSMPack.GeneratedFormats

open System

open FSMPack.Format
open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

#nowarn "0025"

let mutable initialized = false


type FMT_TestProject_Types_Foo() =
    interface Format<TestProject.Types.Foo> with
        member _.Write bw (v: TestProject.Types.Foo) =
            writeMapFormat bw 1
            writeValue bw (RawString "num")
            writeValue bw (Integer v.num)

        member _.Read (br, bytes) =
            let count = 1
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable num = Unchecked.defaultof<System.Int32>
            while items < count do
                match readValue br &bytes with
                | RawString _k ->
                    match _k with
                    | "num" ->
                        let (Integer num') = readValue br &bytes
                        num <- num'
                    | _ -> failwith "Unknown key"
                | notName -> failwith <| "Expected string field name, got " + string notName
                items <- items + 1

            {
                num = num
            }

Cache<TestProject.Types.Foo>.Store (FMT_TestProject_Types_Foo() :> Format<TestProject.Types.Foo>)

type FMT_TestProject_Types_Baz() =
    interface Format<TestProject.Types.Baz> with
        member _.Write bw (v: TestProject.Types.Baz) =
            writeMapFormat bw 2
            writeValue bw (RawString "word")
            writeValue bw (RawString v.word)
            writeValue bw (RawString "bar")
            Cache<TestProject.Types.Bar>.Retrieve().Write bw v.bar

        member _.Read (br, bytes) =
            let count = 2
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable word = Unchecked.defaultof<System.String>
            let mutable bar = Unchecked.defaultof<TestProject.Types.Bar>
            while items < count do
                match readValue br &bytes with
                | RawString _k ->
                    match _k with
                    | "word" ->
                        let (RawString word') = readValue br &bytes
                        word <- word'
                    | "bar" ->
                        let bar' = Cache<TestProject.Types.Bar>.Retrieve().Read(br, bytes)
                        bar <- bar'
                    | _ -> failwith "Unknown key"
                | notName -> failwith <| "Expected string field name, got " + string notName
                items <- items + 1

            {
                word = word
                bar = bar
            }

Cache<TestProject.Types.Baz>.Store (FMT_TestProject_Types_Baz() :> Format<TestProject.Types.Baz>)

type FMT_TestProject_Types_Quix() =
    interface Format<TestProject.Types.Quix> with
        member _.Write bw (v: TestProject.Types.Quix) =
            writeMapFormat bw 2
            writeValue bw (RawString "baz")
            Cache<TestProject.Types.Baz>.Retrieve().Write bw v.baz
            writeValue bw (RawString "a")
            writeValue bw (Integer v.a)

        member _.Read (br, bytes) =
            let count = 2
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable baz = Unchecked.defaultof<TestProject.Types.Baz>
            let mutable a = Unchecked.defaultof<System.Int32>
            while items < count do
                match readValue br &bytes with
                | RawString _k ->
                    match _k with
                    | "baz" ->
                        let baz' = Cache<TestProject.Types.Baz>.Retrieve().Read(br, bytes)
                        baz <- baz'
                    | "a" ->
                        let (Integer a') = readValue br &bytes
                        a <- a'
                    | _ -> failwith "Unknown key"
                | notName -> failwith <| "Expected string field name, got " + string notName
                items <- items + 1

            {
                baz = baz
                a = a
            }

Cache<TestProject.Types.Quix>.Store (FMT_TestProject_Types_Quix() :> Format<TestProject.Types.Quix>)

type FMT_TestProject_Types_Bar() =
    interface Format<TestProject.Types.Bar> with
        member _.Write bw (v: TestProject.Types.Bar) =
            match v with
            | TestProject.Types.Bar.BarFoo (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 0)
                Cache<TestProject.Types.Foo>.Retrieve().Write bw x0
            | TestProject.Types.Bar.BarFloat (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                writeValue bw (FloatDouble x0)
            | TestProject.Types.Bar.BarCase ->
                writeArrayFormat bw 1
                writeValue bw (Integer 2)

        member _.Read (br, bytes) =
            let _count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                let x0 = Cache<TestProject.Types.Foo>.Retrieve().Read(br, bytes)
                TestProject.Types.Bar.BarFoo (x0)
            | Integer 1 ->
                let (FloatDouble x0) = readValue br &bytes
                TestProject.Types.Bar.BarFloat (x0)
            | Integer 2 ->
                TestProject.Types.Bar.BarCase
            | _ ->
                failwith "Unexpected DU case tag"

Cache<TestProject.Types.Bar>.Store (FMT_TestProject_Types_Bar() :> Format<TestProject.Types.Bar>)

let initialize () =
    FSMPack.Formats.Default.setup ()

    initialized <- true

let write value =
    if not initialized then initialize()

    FSMPack.Format.writeBytes value

let read bytes =
    if not initialized then initialize()

    FSMPack.Format.readBytes bytes

let verifyFormatsKnownTypes () =
    ignore <| Cache<System.Double>.Retrieve()
    ignore <| Cache<System.Int32>.Retrieve()
    ignore <| Cache<System.String>.Retrieve()

let verifyFormatsUnknownTypes () =
    ()
