module FSMPack.Tests.Format

open Expecto

open FSMPack.Format
open FSMPack.Tests.Utility
open FSMPack.Tests.Types.Record
open FSMPack.Tests.Types.DU

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

[<Tests>]
let recordTests =
    let formatInnerType = FormatMyInnerType() :> Format<MyInnerType>
    let formatTestType = FormatMyTestType() :> Format<MyTestType>

    Cache<MyInnerType>.Store formatInnerType
    Cache<MyTestType>.Store formatTestType

    testList "Format.Record" [
        testCase "can roundtrip" <| fun _ ->
            let testRecord = {
                C = "hi"
            }
            
            "Simple record can roundtrip"
            |> roundtripFormat (Cache<MyInnerType>.Retrieve()) testRecord

        testCase "can roundtrip nested" <| fun _ ->
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

[<Tests>]
let duTests =
    let formatInnerDU = FormatMyInnerDU() :> Format<MyInnerDU>
    let formatDU = FormatMyDU() :> Format<MyDU>

    Cache<MyInnerDU>.Store formatInnerDU
    Cache<MyDU>.Store formatDU

    testList "Format.DU" [
        testCase "can roundtrip" <| fun _ ->
            "Single case du"
            |> roundtripFormat
                (Cache<MyInnerDU>.Retrieve())
                MyInnerDU.A
            
            "Simple du case"
            |> roundtripFormat
                (Cache<MyInnerDU>.Retrieve())
                (MyInnerDU.B 1)

        testCase "can roundtrip nested" <| fun _ ->
            "Nested single case DU"
            |> roundtripFormat
                (Cache<MyDU>.Retrieve())
                (MyDU.D MyInnerDU.A)

            "Nested simple DU"
            |> roundtripFormat
                (Cache<MyDU>.Retrieve())
                (MyDU.D (MyInnerDU.B 1))

    ]
