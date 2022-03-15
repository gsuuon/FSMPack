module FSMPack.Tests.Integer

open System
open Expecto

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

open FSMPack.Tests.BufWriter
open FSMPack.Tests.Utility
open FSMPack.Utility.Byte

[<Tests>]
let readWriteTests =
    let tinyPairs = [
        [| 0b00000000uy
        |], Integer 0
        [| 0b00000001uy
        |], Integer 1
        [| 0b01111111uy
        |], Integer 0b01111111
        [| 0b11111111uy
        |], Integer -1
    ]

    let smallPairs = [
        [|
            0xccuy
            255uy
        |], UInteger 255u
        [|
            0xccuy
            128uy
        |], UInteger 128u
        [|
            0xd0uy
            byte (-128y)
        |], Integer -128
    ]

    let testReadPair (bytes, value) = expectBytesRead bytes value
    let testWritePair (bytes, value) = expectValueWrites value bytes

    testList "Integer" [
        testCase "read tiny" <| fun _ ->
            tinyPairs
            |> List.iter testReadPair

        testCase "read small" <| fun _ ->
            smallPairs
            |> List.iter testReadPair

        testCase "write tiny" <| fun _ ->
            tinyPairs
            |> List.iter testWritePair

        testCase "write small" <| fun _ ->
            smallPairs
            |> List.iter testWritePair

        testCase "roundtrip tiny" <| fun _ ->
            tinyPairs
            |> List.map snd
            |> List.iter roundtrip

        testCase "roundtrip small" <| fun _ ->
            smallPairs
            |> List.map snd
            |> List.iter roundtrip
    ]

[<Tests>]
let roundtripTests =
    testList "Integer Roundtrips" [
        testCase "Size limits" <| fun _ ->
            [
                int (SByte.MinValue)
                int (SByte.MaxValue)
                int (Byte.MaxValue)
                int (Byte.MinValue)
                int (Int16.MinValue)
                int (Int16.MaxValue)
                Int32.MaxValue
                Int32.MinValue
            ]
            |> List.map Integer
            |> List.iter roundtrip

        testCase "Int64 size limits" <| fun _ ->
            [
                Int64.MaxValue
                Int64.MinValue
            ]
            |> List.map Integer64
            |> List.iter roundtrip

        testProperty "property" <| fun x ->
            roundtrip (Integer x)
        ]
