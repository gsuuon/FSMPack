module FSMPack.Tests.Compile.Generator

open Expecto
open FSMPack.Format

open FSMPack.Write
open FSMPack.Read
open System

let roundtripFormat (f: Format<'T>) v message =
    let bw = BufWriter.Create 0

    f.Write bw v

    let written = bw.GetWritten()
    let read = f.Read (BufReader.Create(), ReadOnlySpan(written))

    Expect.equal read v message

[<FTests>]
let tests =
    let formatInnerType = FormatMyInnerType() :> Format<MyInnerType>
    let formatTestType = FormatMyTestType() :> Format<MyTestType>

    Cache<MyInnerType>.Store formatInnerType
    Cache<MyTestType>.Store formatTestType

    testList "Format" [
        testCase "Record can roundtrip" <| fun _ ->
            let testRecord = {
                C = "hi"
            }
            
            "Simple record can roundtrip"
            |> roundtripFormat formatInnerType testRecord

        testCase "Nested record can roundtrip" <| fun _ ->
            let testRecord = {
                A = 2
                B = 3.1
                inner = {
                    C = "hi"
                }
            }

            "Nested record can roundtrip"
            |> roundtripFormat formatTestType testRecord
    ]

    (* testList "Generator" [ *)
    (*     testCase "records roundtrip" <| fun _ -> *)
    (*         () *)
    (* ] *)
