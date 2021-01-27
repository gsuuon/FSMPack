module FSMPack.Tests.Format

open Expecto

open FSMPack.Format
open FSMPack.Tests.Utility
open FSMPack.Tests.Types.Record
open FSMPack.Tests.Types.DU

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

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

    setupBasicFormatters()

    testList "Generic formatter" [
        testCase "Roundtrip generic record of value type" roundtripGenericOfValue
        testCase "Roundtrip generic record of reference type" roundtripGenericOfReference
    ]
