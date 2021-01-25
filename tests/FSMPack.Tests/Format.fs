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

        testCase "can roundtrip multi-field" <| fun _ ->
            "Multiple field DU case"
            |> roundtripFormat
                (Cache<MyDU>.Retrieve())
                (MyDU.C ("hi", 2.0))
    ]
    
[<FTests>]
let genericTests =
    Cache<MyGenericRecord<_>>.StoreGeneric
        typedefof<FormatMyGenericRecord<_>>

    Cache<MyInnerType>.Store 
        (FormatMyInnerType() :> Format<MyInnerType>)

    Cache<string>.Store
      { new Format<string> with
        member _.Write bw (v: string) =
            writeValue bw (RawString v)
        member _.Read (br, bytes) =
            let (RawString x) = readValue br &bytes
            x } 

    Cache<float>.Store
      { new Format<float> with
        member _.Write bw (v: float) =
            writeValue bw (FloatDouble v)
        member _.Read (br, bytes) =
            let (FloatDouble x) = readValue br &bytes
            x } 


    testList "Generic formatter" [
        testCase "Roundtrip generic record of value type" <| fun _ ->
            "Simple generic record of string"
            |> roundtripFormat
                (Cache<MyGenericRecord<string>>.Retrieve())
                { foo = "Hi" }

            "Simple generic record of float"
            |> roundtripFormat
                (Cache<MyGenericRecord<float>>.Retrieve())
                { foo = 12.3 }

        testCase "Roundtrip generic record of reference type" <| fun _ ->
            "Simple generic record of record"
            |> roundtripFormat
                (Cache<MyGenericRecord<MyInnerType>>.Retrieve())
                { foo = { C = "Hi" } }
    ]
