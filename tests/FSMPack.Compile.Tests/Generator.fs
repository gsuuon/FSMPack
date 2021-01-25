module FSMPack.Tests.Compile.Generator

open System
open Expecto

open FSMPack.Format
open FSMPack.Write
open FSMPack.Read

[<Tests>]
let tests =
    testList "Generator produces code matching format" [
        testCase "Generator produces strings" <| fun _ ->
            ()
    ]
