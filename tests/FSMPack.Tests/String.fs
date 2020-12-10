module FSMPack.Tests.String

open System
open Expecto

open FSMPack.Spec
open FSMPack.Tests.Utility

let stringOfSize size =
    repeatStr "" "a" size

[<Tests>]
let tests =
    testList "String" [
        testCase "roundtrip tiny" <| fun _ ->
            roundtrip (RawString "h")
            roundtrip (RawString "hello")
            roundtrip (RawString "hello there")

            stringOfSize (100) |> RawString |> roundtrip

        testCase "roundtrip small" <| fun _ ->

            stringOfSize (100) |> RawString |> roundtrip

            stringOfSize (int Byte.MaxValue - 1)
            |> RawString |> roundtrip

        testCase "roundtrip medium" <| fun _ ->
            stringOfSize (int Int16.MaxValue - 1)
            |> RawString |> roundtrip

        // Just uh.. trust me
        // TODO read/write stream
        (* testCase "roundtrip big" <| fun _ -> *)
        (*     stringOfSize (Int32.MaxValue - 1) *)
        (*     |> RawString |> roundtrip *)
    ]
