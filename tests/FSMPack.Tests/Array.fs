module FSMPack.Tests.Array

open Expecto

open FSMPack.Spec
open FSMPack.Write
open FSMPack.Tests.Utility

[<Tests>]
let tests =
    let tinyArray = [|
        Integer 1
        Integer64 1L
        Boolean false
        RawString "hi" |]

    let tinyBytes =
        Array.append
            [| byte Format.FixArray ||| 4uy |]
            (tinyArray
            |> Array.map writeValueToBytes
            |> Array.concat)

    testList "Array" [
        testCase "reads tiny" <| fun _ ->
            expectBytesRead
                tinyBytes
                (ArrayCollection tinyArray)

        testCase "writes tiny" <| fun _ ->
            expectValueWrites
                (ArrayCollection tinyArray)
                tinyBytes

        testCase "roundtrips tiny" <| fun _ ->
            let seed = System.Random()

            [|generateRandomValue seed|]
            |> ArrayCollection
            |> roundtrip

            [|0..14|]
            |> Array.map (fun _ -> generateRandomValue seed)
            |> ArrayCollection
            |> roundtrip

        testCase "roundtrips medium" <| fun _ ->
            let seed = System.Random()

            [|0..200|]
            |> Array.map (fun _ -> generateRandomValue seed)
            |> ArrayCollection
            |> roundtrip

            [|0..(pown 2 16) - 1|]
            |> Array.map (fun _ -> generateRandomValue seed)
            |> ArrayCollection
            |> roundtrip
    ]
