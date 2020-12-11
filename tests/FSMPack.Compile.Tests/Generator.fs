module FSMPack.Tests.Compile.Generator

open Expecto
open FSMPack.Format

open FSMPack.Write
open FSMPack.Read
open System

[<Tests>]
let tests =
    testList "Generator produces code matching format" []
