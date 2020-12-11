module FSMPack.Tests.Format

open Expecto

open FSMPack.Format
open FSMPack.Tests.Utility
open FSMPack.Tests.Types.Record

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

[<Tests>]
let recordTests =
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
            |> roundtripFormat (Cache<MyInnerType>.Retrieve()) testRecord

        testCase "Nested record can roundtrip" <| fun _ ->
            let testRecord = {
                A = 2
                B = 3.1
                inner = {
                    C = "hi"
                }
            }

            "Nested record can roundtrip"
            |> roundtripFormat (Cache<MyTestType>.Retrieve())  testRecord
    ]
