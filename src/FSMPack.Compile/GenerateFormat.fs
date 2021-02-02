module FSMPack.Compile.GenerateFormat
/// Generates code as strings for a type

open System
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

open FSMPack.Compile.Generator.Common
open FSMPack.Compile.Generator.Record
open FSMPack.Compile.Generator.DU

let generateFormat (typ: Type) =
    if FSharpType.IsRecord typ then
        generateFormatRecord typ
    else if FSharpType.IsUnion typ then
        generateFormatDU typ
    else
        "// Unknown type"

let addFormattersFileHeader (formatters: string list) =
    header +
        (formatters
        |> String.concat "\n\n")
