module FSMPack.Tests.Map

open Expecto

open FSMPack.Spec
open FSMPack.Tests.Utility

[<Tests>]
let tests =
    let tinyMap =
        (dict [ Integer 0, Boolean false ])
        |> MapCollection 

    let tinyBytes =
        [|
            0b10000000uy ||| 1uy
            0uy
            0b11000010uy
        |]


    testList "Map" [
        testCase "write tiny map" <| fun _ ->
            expectValueWrites tinyMap tinyBytes

        testCase "read tiny map" <| fun _ ->
            expectBytesRead tinyBytes tinyMap

        testCase "roundtrip tiny" <| fun _ ->
            roundtrip tinyMap

            let seed = System.Random()

            [0..10]
            |> List.map
                (fun _ ->
                    generateRandomSimpleValue seed
                        // Map as key is broken due to compare/equal
                    , generateRandomValue seed )
            |> dict
            |> MapCollection
            |> roundtrip
    ]
