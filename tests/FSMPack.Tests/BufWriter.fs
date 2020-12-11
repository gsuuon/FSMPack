module FSMPack.Tests.BufWriter

open System
open System.Buffers
open Expecto

open FSMPack.Write
open FSMPack.Tests.Utility

[<Tests>]
let contractTests =
    let getSpanAndCheckGrowth (bw: BufWriter) size =
        sprintf "GetSpan %i" size
        |> Expect.isGreaterThanOrEqual
            (bw.GetSpan(size).Length) size 

        sprintf "Backing array grows %i" size
        |> Expect.isGreaterThanOrEqual
            bw.buffer.Length size

    testList "BufWriter.IBufferWriter contract" [
        testCase "Can produce a span" <| fun _ ->
            let bw = BufWriter.Create 1
            
            [ 0 .. 5 ]
            |> List.iter
                (getSpanAndCheckGrowth bw)

        testCase "Can grow span" <| fun _ ->
            let bw = BufWriter.Create 1

            [ 10 .. 10 .. 100 ]
            |> List.iter
                (getSpanAndCheckGrowth bw)

            [ 1000
              10000
              100000 ]
            |> List.iter
                (getSpanAndCheckGrowth bw)
    ]

[<Tests>]
let writeTests =
    testList "BufWriter" [
        testCase "Can write a byte" <| fun _ ->
            let bw = BufWriter.Create 0
            bw.Write (ReadOnlySpan [|0uy|])
            let bytes = bw.GetWritten()

            "Writes 0"
            |> Expect.equal bytes [|0uy|]

        testCase "Can write bytes" <| fun _ ->
            let bw = BufWriter.Create 0

            let bytesToWrite = ReadOnlySpan [|0uy..100uy|]

            bw.Write bytesToWrite

            let bytes = bw.GetWritten()

            "Writes 0 to 100"
            |> Expect.equal bytes [|0uy..100uy|]

        testCase "Can write multiple times" <| fun _ ->
            let bw = BufWriter.Create 0

            bw.Write (ReadOnlySpan [|0uy..4uy|])

            "Wrote 5 bytes"
            |> Expect.equal (bw.GetWritten().Length) 5 

            bw.Write (ReadOnlySpan [|5uy..9uy|])

            "Wrote 10 bytes total"
            |> Expect.equal (bw.GetWritten().Length) 10

            "Bytes are correct"
            |> Expect.equal (bw.GetWritten()) [|0uy..9uy|]
    ]
