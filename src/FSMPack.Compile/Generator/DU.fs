module FSMPack.Compile.Generator.DU

open System
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

open FSMPack.Compile.Generator.Common

type DUCase = {
    name : string
    fieldTyp : Type option
}

let getCases (typ: Type) = [
    for uci in FSharpType.GetUnionCases typ do

