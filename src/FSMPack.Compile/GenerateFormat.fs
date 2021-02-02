module FSMPack.Compile.GenerateFormat

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

[<AutoOpen>]
module Helpers =
    let prependText text body =
        text + "\n" + body

let produceFormattersText types =
    let typePaths = 
        types
        |> List.map (fun (typ: Type) ->
            sprintf "%A: %s %A"
                typ
                typ.Namespace
                typ.DeclaringType
                )
        |> String.concat "\n"

    printf "Got type paths:\n%s\n" typePaths
    
    types
    |> List.map generateFormat
    |> String.concat "\n"
    |> prependText Generator.Common.header
