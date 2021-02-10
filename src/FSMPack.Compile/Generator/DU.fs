module FSMPack.Compile.Generator.DU

open System
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

open FSMPack.Compile.Generator.Common

type DUField =
  { typeFullName : string
    typ : Type }

type DUCase =
  { name : string
    tag : int
    fields : DUField array }

let getField (fi: PropertyInfo) =
  { typeFullName = TypeName.field fi.PropertyType
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
    let simpleName =
        typ
        |> TypeName.Transform.simpleName
        |> TypeName.Transform.lexName

    let nameWithGenArgs =
        simpleName
        |> TypeName.Transform.addNamedArgs typ

    let fmtTypName = formatTypeName typ

    $"""open {getTypeOpenPath typ}

type {fmtTypName}() =
{__}interface Format<{nameWithGenArgs}> with
{__}{__}member _.Write bw (v: {nameWithGenArgs}) =
{__}{__}{__}match v with
{ [ for c in cases do
        yield $"| {simpleName}.{c.name}{destructFields c} ->"
        yield $"{__}writeArrayFormat bw {c.fields.Length + 1}"
        yield $"{__}writeValue bw (Integer {c.tag})"
        yield! 
            c.fields
            |> Array.mapi
                (fun idx f ->
                    match msgpackTypes.TryGetValue f.typ with
                    | true, mpType ->
                        $"{__}writeValue bw ({mpType} x{idx})"
                    | false, _ ->
                        $"{__}Cache<{f.typeFullName}>.Retrieve().Write bw x{idx}")
        ]
        |> List.map (indentLine 3)
        |> String.concat "\n" }

{__}{__}member _.Read (br, bytes) =
{__}{__}{__}let count = readArrayFormatCount br &bytes

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
        yield $"{__}{nameWithGenArgs}.{c.name}{destructFields c}"
    ]
    |> List.map (indentLine 3)
    |> String.concat "\n" }
{__}{__}{__}| _ ->
{__}{__}{__}{__}failwith "Unexpected DU case tag"

{writeCacheFormatLine typ fmtTypName}
"""
