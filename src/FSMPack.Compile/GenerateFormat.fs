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
open FSMPack.Compile.Generator.Enum

let header = """module FSMPack.GeneratedFormats

open System

open FSMPack.Format
open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

#nowarn "0025"

let mutable initialized = false

"""

let footer = """
let initialize () =
    FSMPack.Formats.Default.setup ()

    initialized <- true

let write value =
    if not initialized then initialize()

    FSMPack.Format.writeBytes value

let read bytes =
    if not initialized then initialize()

    FSMPack.Format.readBytes bytes
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
    @ List.map generateFormatEnum
        categorizedTypes.enumTypes
    |> String.concat "\n"
    |> prependText header
    |> fun t -> t + footer

let writeText outpath text =
    File.WriteAllText (outpath, text)

let generateFormatsText (categorizedTypes: CategorizedTypes) =
    let verifyFormatsFnsText =
        let produceCacheRetrieveCalls types =
            types
            |> List.map (fun typ ->
                typ
                |> TypeName.getFullCanonName
                |> TypeName.Transform.addAnonArgs typ
                |> sprintf "    ignore <| Cache<%s>.Retrieve()"
                )

        let produceUnitFn fnName (fnBodyLines: string list) =
            fnBodyLines
            |> String.concat "\n"
            |> (+) (sprintf "\nlet %s () =\n" fnName)
            |> fun t ->
                if fnBodyLines.Length = 0 then
                    t + "    ()\n"
                else
                    t + "\n"
            
        (categorizedTypes.knownTypes
        |> produceCacheRetrieveCalls
        |> produceUnitFn "verifyFormatsKnownTypes"
        )

        +

        (categorizedTypes.unknownTypes
        |> produceCacheRetrieveCalls
        |> produceUnitFn "verifyFormatsUnknownTypes"
        )

    categorizedTypes
    |> produceFormatsText
    |> fun formatsText -> formatsText + verifyFormatsFnsText
