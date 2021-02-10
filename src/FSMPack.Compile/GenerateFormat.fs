module FSMPack.Compile.GenerateFormat

open System
open System.IO
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

open FSMPack.Compile.AnalyzeInputAssembly
open FSMPack.Compile.Generator.Common
open FSMPack.Compile.Generator.Record
open FSMPack.Compile.Generator.DU

(* NOTE
Using the mutable _initStartupCode to kick off initialization code of the generated module w/o reflection.
Is there a better way to do this? *)
let header = """module FSMPack.GeneratedFormats

open System

open FSMPack.Format
open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

#nowarn "0025"

let mutable _initStartupCode = 0

"""

let footer = """
let initialize () =
    FSMPack.BasicFormats.setup ()

    _initStartupCode
"""

let addFormattersFileHeader (formatters: string list) =
    header +
        (formatters
        |> String.concat "\n\n")

[<AutoOpen>]
module Helpers =
    let prependText text body =
        text + "\n" + body

let produceFormatsText categorizedTypes =
    List.map generateFormatRecord
        categorizedTypes.recordTypes
    @ List.map generateFormatDU
        categorizedTypes.duTypes
    |> String.concat "\n"
    |> prependText header
    |> fun t -> t + footer

let writeText outpath text =
    File.WriteAllText (outpath, text)
