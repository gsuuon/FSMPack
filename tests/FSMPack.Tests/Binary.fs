module FSMPack.Tests.Binary

open System
open Expecto

open FSMPack.Spec
open FSMPack.Tests.Utility

[<Tests>]
let tests =
    testList "Binary" [
        testCase "roundtrip tiny" <| fun _ ->
            roundtrip (Binary [||])
            roundtrip (Binary [|0uy|])
            roundtrip (Binary [|0uy..byte SByte.MaxValue|])

        testCase "roundtrip medium" <| fun _ ->
            [|0..(pown 2 16) - 2|]
            |> Array.map (fun x -> byte (x % int Byte.MaxValue) )
            |> Binary
            |> roundtrip

        // TODO stream for 2 ^ 32 - 1 sized binary
    ]
