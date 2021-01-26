module FSMPack.Compile.Generate
/// Generates code as strings for a type

open System
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

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
    let header = """module FSMPack.GeneratedFormatters

open System

open FSMPack.Format
open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

open FSMPack.Tests.Types.Record

#nowarn "0025"

"""

    header +
        (formatters
        |> String.concat "\n\n")

