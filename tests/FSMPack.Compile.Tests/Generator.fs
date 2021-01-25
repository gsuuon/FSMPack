module FSMPack.Tests.Compile.Generator

open System
open Expecto

open FSMPack.Format
open FSMPack.Write
open FSMPack.Read
open FSMPack.Compile

[<Tests>]
let tests =
    testList "Generator produces code matching format" [
        testCase "Generator produces strings" <| fun _ ->
            ()
    ]
