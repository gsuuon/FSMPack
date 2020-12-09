module FSMPack.Tests.Float

open System
open Expecto
open FsCheck

open FSMPack.Spec
open FSMPack.Tests.Utility

[<Tests>]
let tests =
    testList "Float" [
        testCase "roundtrips single" <| fun _ ->
            roundtrip (FloatSingle 0.0f)
            roundtrip (FloatSingle 1230.123f)
            roundtrip (FloatSingle Single.MinValue)
            roundtrip (FloatSingle Single.MaxValue)

        testCase "roundtrips double" <| fun _ ->
            roundtrip (FloatDouble 312321.123124)
            roundtrip (FloatDouble Double.MinValue)
            roundtrip (FloatDouble Double.MaxValue)

        testProperty "roundtrips single property"
            <| fun (x: NormalFloat) ->
                roundtrip (FloatSingle (single x.Get))

        testProperty "roundtrips double property"
            <| fun (x: NormalFloat) ->
                roundtrip (FloatDouble x.Get)
    ]
