module FSMPack.Tests.Format

open Expecto

open FSMPack.Format
open FSMPack.Tests.Utility
open FSMPack.Tests.Types.Record
open FSMPack.Tests.Types.DU

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

module FormatTests =
    let roundtripSimpleRecord _ =
        "Simple record can roundtrip"
        |> roundtripFormat (Cache<MyInnerType>.Retrieve())
            {
                C = "hi"
            }

    let roundtripNestedRecord _ =
        "Nested record can roundtrip"
        |> roundtripFormat (Cache<MyTestType>.Retrieve()) {
            A = 2
            B = 3.1
            inner = {
                C = "hi"
            }
        }

    let roundtripSimpleDU _ =
        "Single case du"
        |> roundtripFormat
            (Cache<MyInnerDU>.Retrieve())
            MyInnerDU.A
        
        "Simple du case"
        |> roundtripFormat
            (Cache<MyInnerDU>.Retrieve())
            (MyInnerDU.B 1)

    let roundtripNestedDU _ =
        "Nested single case DU"
        |> roundtripFormat
            (Cache<MyDU>.Retrieve())
            (MyDU.D MyInnerDU.A)

        "Nested simple DU"
        |> roundtripFormat
            (Cache<MyDU>.Retrieve())
            (MyDU.D (MyInnerDU.B 1))

    let roundtripMultiFieldDU _ =
        "Multiple field DU case"
        |> roundtripFormat
            (Cache<MyDU>.Retrieve())
            (MyDU.C ("hi", 2.0))

    let roundtripGenericOfValue _ =
        "Simple generic record of string"
        |> roundtripFormat
            (Cache<MyGenericRecord<string>>.Retrieve())
            { foo = "Hi" }

        "Simple generic record of float"
        |> roundtripFormat
            (Cache<MyGenericRecord<float>>.Retrieve())
            { foo = 12.3 }

    let roundtripGenericOfReference _ =
        "Simple generic record of record"
        |> roundtripFormat
            (Cache<MyGenericRecord<MyInnerType>>.Retrieve())
            { foo = { C = "Hi" } }

    let setupStandardFormatters () =
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

open FormatTests

[<Tests>]
let recordTests =
    let formatInnerType = FormatMyInnerType() :> Format<MyInnerType>
    let formatTestType = FormatMyTestType() :> Format<MyTestType>

    Cache<MyInnerType>.Store formatInnerType
    Cache<MyTestType>.Store formatTestType

    testList "Format.Record" [
        testCase "can roundtrip" roundtripSimpleRecord
        testCase "can roundtrip nested" roundtripNestedRecord
    ]

[<Tests>]
let duTests =
    let formatInnerDU = FormatMyInnerDU() :> Format<MyInnerDU>
    let formatDU = FormatMyDU() :> Format<MyDU>

    Cache<MyInnerDU>.Store formatInnerDU
    Cache<MyDU>.Store formatDU

    testList "Format.DU" [
        testCase "can roundtrip" roundtripSimpleDU
        testCase "can roundtrip nested" roundtripNestedDU
        testCase "can roundtrip multi-field" roundtripMultiFieldDU
    ]
    
[<Tests>]
let genericTests =
    Cache<MyGenericRecord<_>>.StoreGeneric
        typedefof<FormatMyGenericRecord<_>>

    Cache<MyInnerType>.Store 
        (FormatMyInnerType() :> Format<MyInnerType>)

    setupStandardFormatters()

    testList "Generic formatter" [
        testCase "Roundtrip generic record of value type" roundtripGenericOfValue
        testCase "Roundtrip generic record of reference type" roundtripGenericOfReference
    ]
