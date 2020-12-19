module FSMPack.Compile.Generator
/// Generates code as strings for a type

open System
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

open FSMPack.Compile.Generator.Record

let generateFormat (typ: Type) =
    if FSharpType.IsRecord typ then
        generateFormatRecord typ
    else
        "()"
