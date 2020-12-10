module FSMPack.Tests.Binary

open System
open Expecto

open FSMPack.Spec
open FSMPack.Tests.Utility

[<Tests>]
let tests =
    testList "Binary" [
        testCase "roundtrip tiny" <| fun _ ->
            roundtrip (RawBinary [||])
            roundtrip (RawBinary [|0uy|])
            roundtrip (RawBinary [|0uy..byte SByte.MaxValue|])

        testCase "roundtrip medium" <| fun _ ->
            [|0..(pown 2 16) - 2|]
            |> Array.map (fun x -> byte (x % int Byte.MaxValue) )
            |> RawBinary
            |> roundtrip

        // TODO stream for 2 ^ 32 - 1 sized binary
    ]
