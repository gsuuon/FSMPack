module FSMPack.Tests.FormatTests

open System.Collections.Generic

open Expecto

open FSMPack.Format

open FSMPack.Tests.Utility
open FSMPack.Tests.Types.Record
open FSMPack.Tests.Types.DU
open FSMPack.Tests.Types.Collection

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

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

module TestCases =
    let records =
        testList "Format.Record" [
            testCase "can roundtrip" roundtripSimpleRecord
            testCase "can roundtrip nested" roundtripNestedRecord
        ]

    let DUs =
        testList "Format.DU" [
            testCase "can roundtrip" roundtripSimpleDU
            testCase "can roundtrip nested" roundtripNestedDU
            testCase "can roundtrip multi-field" roundtripMultiFieldDU
        ]

    let generics =
        testList "Format.Generic" [
            testCase "Roundtrip generic record of value type" roundtripGenericOfValue
            testCase "Roundtrip generic record of reference type" roundtripGenericOfReference
        ]

    let collections =
        testList "Format.Collection" [
            testCase "Net" <| fun _ ->
                "roundtrip IDictionary"
                |> roundtripFormatExpect
                    (Cache<IDictionary<string, int>>.Retrieve())
                    (dict [
                        "x", 1
                        "y", 10
                        "z", 100
                    ] )
                    expectIDictEqual

                "roundtrip Dictionary"
                |> roundtripFormatExpect
                    (Cache<Dictionary<string, int>>.Retrieve())
                    (Dictionary (dict [
                        "a", 3
                        "b", 12
                        "c", 50
                    ] ) )
                    expectIDictEqual

            testCase "FSharp" <| fun _ ->
                "roundtrip Map"
                |> roundtripFormat
                    (Cache<Map<int,string>>.Retrieve())
                    (Map.ofList [
                        0, "hi"
                        1, "bye"
                        4, "hi again"] )

                "roundtrip List"
                |> roundtripFormat
                    (Cache<_ list>.Retrieve())
                    [ "a"; "foo"; "one" ]


                "roundtrip different List"
                |> roundtripFormat
                    (Cache<_ list>.Retrieve())
                    [ 0; 10; 12 ]

                "roundtrip Array"
                |> roundtripFormat
                    (Cache<_ array>.Retrieve())
                    [| "a"; "foo"; "one" |]

                "roundtrip different Array"
                |> roundtripFormat
                    (Cache<_ array>.Retrieve())
                    [| 1; 9; 17 |]
        ]

    let basicFormats =
        testList "Format.Basic" [
            testCase "Generics" <| fun _ ->
                "roundtrip Option"
                |> roundtripFormat
                    (Cache<_ option>.Retrieve())
                    (Some "hi")

            testCase "Basics" <| fun _ ->
                "roundtrip bool"
                |> roundtripFormat
                    (Cache<bool>.Retrieve())
                    false

                "roundtrip unit"
                |> roundtripFormat
                    (Cache<unit>.Retrieve())
                    ()
        ]
