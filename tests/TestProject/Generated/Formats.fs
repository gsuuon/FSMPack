module FSMPack.GeneratedFormats

open System

open FSMPack.Format
open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

#nowarn "0025"

let mutable _initStartupCode = 0


open TestProject.Types

type FormatQuix() =
    interface Format<Quix> with
        member _.Write bw (v: Quix) =
            writeMapFormat bw 2
            writeValue bw (RawString "baz")
            Cache<Baz>.Retrieve().Write bw v.baz
            writeValue bw (RawString "b")
            writeValue bw (Boolean v.b)

        member _.Read (br, bytes) =
            let count = 2
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable baz = Unchecked.defaultof<Baz>
            let mutable b = Unchecked.defaultof<Boolean>
            while items < count do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "baz" ->
                        baz <- Cache<Baz>.Retrieve().Read(br, bytes)
                    | "b" ->
                        let (Boolean x) = readValue br &bytes
                        b <- x
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                baz = baz
                b = b
            }

Cache<Quix>.Store (FormatQuix() :> Format<Quix>)

open TestProject.Types

type FormatBaz() =
    interface Format<Baz> with
        member _.Write bw (v: Baz) =
            writeMapFormat bw 2
            writeValue bw (RawString "word")
            writeValue bw (RawString v.word)
            writeValue bw (RawString "bar")
            Cache<Bar>.Retrieve().Write bw v.bar

        member _.Read (br, bytes) =
            let count = 2
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable word = Unchecked.defaultof<String>
            let mutable bar = Unchecked.defaultof<Bar>
            while items < count do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "word" ->
                        let (RawString x) = readValue br &bytes
                        word <- x
                    | "bar" ->
                        bar <- Cache<Bar>.Retrieve().Read(br, bytes)
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                word = word
                bar = bar
            }

Cache<Baz>.Store (FormatBaz() :> Format<Baz>)

open TestProject.Types

type FormatBar() =
    interface Format<Bar> with
        member _.Write bw (v: Bar) =
            match v with
            | Bar.BarFoo (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 0)
                Cache<TestProject.Types.Foo>.Retrieve().Write bw x0
            | Bar.BarFloat (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                writeValue bw (FloatDouble x0)
            | Bar.BarCase ->
                writeArrayFormat bw 1
                writeValue bw (Integer 2)

        member _.Read (br, bytes) =
            let count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                let x0 = Cache<TestProject.Types.Foo>.Retrieve().Read(br, bytes)
                Bar.BarFoo (x0)
            | Integer 1 ->
                let (FloatDouble x0) = readValue br &bytes
                Bar.BarFloat (x0)
            | Integer 2 ->
                Bar.BarCase
            | _ ->
                failwith "Unexpected DU case tag"

Cache<Bar>.Store (FormatBar() :> Format<Bar>)

open TestProject.Types

type FormatFoo() =
    interface Format<Foo> with
        member _.Write bw (v: Foo) =
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
            let mutable num = Unchecked.defaultof<Int32>
            while items < count do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "num" ->
                        let (Integer x) = readValue br &bytes
                        num <- x
                    | _ -> failwith "Unknown key"
                items <- items + 1

            {
                num = num
            }

Cache<Foo>.Store (FormatFoo() :> Format<Foo>)

// Unknown type System.String
// Unknown type System.Int32
// Unknown type System.Double
// Unknown type System.Boolean
let initialize () =
    FSMPack.BasicFormats.setup ()

    _initStartupCode
