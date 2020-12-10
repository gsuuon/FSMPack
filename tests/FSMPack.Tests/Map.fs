module FSMPack.Tests.Map

open Expecto

open FSMPack.Spec
open FSMPack.Tests.Utility

/// 0 - 12 basic values
let generateValue =
    function
    | 0 -> Nil
    | 1 -> Boolean false
    | 2 -> Boolean true
    | 3 -> Integer 1
    | 4 -> Integer64 1L
    | 5 -> UInteger 1u
    | 6 -> UInteger64 1UL
    | 7 -> FloatSingle 1.1f
    | 8 -> FloatDouble 1.1
    | 9 -> RawString "one"
    | 10 -> Binary [|1uy|]
    | 11 -> ArrayCollection [| Nil |]
    | 12 -> MapCollection <| dict [ Boolean false, Nil ]
    | x -> Extension (x, [||])

let generateRandomValue (seed: System.Random) =
    generateValue <| seed.Next(13)

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
                    generateRandomValue seed
                    , generateRandomValue seed )
            |> dict
            |> MapCollection
            |> roundtrip
    ]
