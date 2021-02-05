module FSMPack.Compile.GenerateFormat

open System
open System.IO
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
        sprintf "// Unknown type %A" typ

let addFormattersFileHeader (formatters: string list) =
    header +
        (formatters
        |> String.concat "\n\n")

[<AutoOpen>]
module Helpers =
    let prependText text body =
        text + "\n" + body

let produceFormattersText types =
    types
    |> List.map generateFormat
    |> String.concat "\n"
    |> prependText Generator.Common.header
    |> fun t -> t + footer

let writeFormatterText outpath text =
    File.WriteAllText (outpath, text)
