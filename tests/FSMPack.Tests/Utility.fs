module FSMPack.Tests.Utility

open System
open System.Collections.Generic
open Expecto

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

let newBufReader () = { idx = 0 }

let newBufWriter size =
  { idx = 0
    buffer = [||]
    initialSize = size }

let rec valueEquals actual expected =
    match actual, expected with
    | Nil, Nil -> true
    | Boolean a, Boolean b -> a = b
    | Integer a, Integer b -> a = b
    | Integer64 a, Integer64 b -> a = b
    | UInteger a, UInteger b -> a = b
    | UInteger64 a, UInteger64 b -> a = b
    | FloatSingle a, FloatSingle b -> a = b
    | FloatDouble a, FloatDouble b -> a = b
    | RawString a, RawString b -> a = b
    | Binary a, Binary b -> a = b
    | ArrayCollection a, ArrayCollection b ->
        Array.zip a b
        |> Array.forall
            (fun (x, y) -> valueEquals x y)

    | Extension (ta, da), Extension (tb, db) ->
        ta = tb && da = db

    | MapCollection a, MapCollection b ->
        let ca = Dictionary a
        let cb = Dictionary b

        for key in a.Keys do
            if valueEquals ca.[key] cb.[key] then
                ignore <| ca.Remove key
                ignore <| cb.Remove key

        ca.Count = 0 && cb.Count = 0

    | _, _ -> false

let expectValueEquals actual expected message =
    if not (valueEquals actual expected) then
        // TODO not sure how to use valueEquals and still get the pretty printing
        Expect.equal actual expected message

let expectBytesRead bytes expected =
    let readBytes = ReadOnlySpan bytes
    let actual = readValue (newBufReader()) &readBytes

    sprintf "Reads %A" expected
    |> expectValueEquals actual expected 

let expectValueWrites value expected =
    let bw = BufWriter.Create 0

    let actual =
        writeValue bw value
        bw.GetWritten()

    sprintf "Writes %A" value
    |> Expect.equal actual expected

let roundtrip v =
    let actual =
        try
            let buf = BufWriter.Create 0
            writeValue buf v

            let readBytes = ReadOnlySpan (buf.GetWritten())

            readValue
                (newBufReader())
                &readBytes
        with
        | error ->
            raise <| AggregateException
                ([|
                    Exception
                        (sprintf "Roundtrip failed for %A" v)
                    error
                |] )

    sprintf "Roundtrips %A" v
    |> expectValueEquals actual v

let byteToString (byt: byte) =
    Convert.ToString(byt, 2).PadLeft(8, '0')

let repeatStr init incr size =
    let mutable cur = 0
    let mutable x : string = init

    while cur < size do
        x <- x + incr
        cur <- cur + 1

    x

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
