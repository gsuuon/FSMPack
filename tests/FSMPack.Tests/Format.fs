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

    TestCases.records

[<Tests>]
let duTests =
    let formatInnerDU = FormatMyInnerDU() :> Format<MyInnerDU>
    let formatDU = FormatMyDU() :> Format<MyDU>

    Cache<MyInnerDU>.Store formatInnerDU
    Cache<MyDU>.Store formatDU

    TestCases.DUs
    
[<Tests>]
let genericTests =
    Cache<MyGenericRecord<_>>.StoreGeneric
        typedefof<FormatMyGenericRecord<_>>

    Cache<MyInnerType>.Store 
        (FormatMyInnerType() :> Format<MyInnerType>)

    FSMPack.BasicFormatters.setup()

    TestCases.generics
