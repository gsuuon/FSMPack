module FSMPack.Compile.Generator.DU

open System
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

open FSMPack.Compile.Generator.Common

type DUCase =
  { name : string
    tag : int
    fields : Field array }

let getField (fi: PropertyInfo) =
  { typeFullName = TypeName.field fi.PropertyType
    name = fi.Name
    typ = fi.PropertyType }

let getCases (typ: Type) = [
    for uci in FSharpType.GetUnionCases typ do
        yield
          { name = uci.Name
            tag = uci.Tag
            fields =
                uci.GetFields()
                |> Array.map getField } ]

/// (x0, x1, ...xn) or empty string if no fields
let destructFields case =
    if case.fields.Length > 0 then
        $""" ({ case.fields
                |> Array.mapi (fun idx _ -> $"x{idx}" )
                |> String.concat ", " })"""
    else
        ""

let canonTypeName (fullName: string) =
    fullName.Replace ("+", ".")

let generateFormatDU (typ: Type) =
    let cases = getCases typ
    let names = TypeName.getGeneratorNames typ

    $"""type {names.formatTypeNamedArgs}() =
{__}interface Format<{names.dataTypeNamedArgs}> with
{__}{__}member _.Write bw (v: {names.dataTypeNamedArgs}) =
{__}{__}{__}match v with
{ [ for c in cases do
        yield $"| {names.dataType}.{c.name}{destructFields c} ->"
        yield $"{__}writeArrayFormat bw {c.fields.Length + 1}"
        yield $"{__}writeValue bw (Integer {c.tag})"
        yield! 
            c.fields
            |> Array.mapi
                (fun idx f ->
                    getWriteFieldCall f ("x" + string idx) )
            |> Array.map (indentLine 1)
        ]
        |> List.map (indentLine 3)
        |> String.concat "\n" }

{__}{__}member _.Read (br, bytes) =
{__}{__}{__}let _count = readArrayFormatCount br &bytes

{__}{__}{__}match readValue br &bytes with
{ [ for c in cases do
        yield $"| Integer {c.tag} ->"
        yield! 
            c.fields
            |> Array.mapi (fun idx f ->
                match msgpackTypes.TryGetValue f.typ with
                | true, mpType ->
                    $"{__}let ({mpType} x{idx}) = readValue br &bytes"
                | false, _ ->
                    $"{__}let x{idx} = Cache<{f.typeFullName}>.Retrieve().Read(br, bytes)"
            )
        yield $"{__}{names.dataTypeNamedArgs}.{c.name}{destructFields c}"
    ]
    |> List.map (indentLine 3)
    |> String.concat "\n" }
{__}{__}{__}| _ ->
{__}{__}{__}{__}failwith "Unexpected DU case tag"

{writeCacheFormatLine typ names}
"""
