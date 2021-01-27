module FSMPack.Compile.Generator.DU

open System
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

open FSMPack.Compile.Generator.Common

type DUCase = {
    name : string
    tag : int
    fieldInfos : PropertyInfo array
}

let getCases (typ: Type) = [
    for uci in FSharpType.GetUnionCases typ do
        yield
          { name = uci.Name
            tag = uci.Tag
            fieldInfos = uci.GetFields() } ]

let destructFields case =
    if case.fieldInfos.Length > 0 then
        $""" ({ case.fieldInfos
                |> Array.mapi (fun idx _ -> $"x{idx}" )
                |> String.concat ", " })"""
    else
        ""

let canonTypeName (fullName: string) =
    fullName.Replace ("+", ".")

let generateFormatDU (typ: Type) =
    let cases = getCases typ

    $"""
type Format{typ.Name}() =
{__}interface Format<{typ.Name}> with
{__}{__}member _.Write bw (v: {typ.Name}) =
{__}{__}{__}match v with
{ [ for c in cases do
        yield $"| {c.name}{destructFields c} ->"
        yield $"{__}writeArrayFormat bw {c.fieldInfos.Length + 1}"
        yield $"{__}writeValue bw (Integer {c.tag})"
        yield! 
            c.fieldInfos
            |> Array.mapi
                (fun idx f ->
                    match msgpackTypes.TryGetValue f.PropertyType with
                    | true, mpType ->
                        $"{__}writeValue bw ({mpType} x{idx})"
                    | false, _ ->
                        $"{__}Cache<{canonTypeName f.PropertyType.FullName}>.Retrieve().Write bw x{idx}")
        ]
        |> List.map (indentLine 3)
        |> String.concat "\n" }

{__}{__}member _.Read (br, bytes) =
{__}{__}{__}let count = readArrayFormatCount br &bytes

{__}{__}{__}match readValue br &bytes with
{ [ for c in cases do
        yield $"| Integer {c.tag} ->"
        yield! 
            c.fieldInfos
            |> Array.mapi (fun idx f ->
                match msgpackTypes.TryGetValue f.PropertyType with
                | true, mpType ->
                    $"{__}let ({mpType} x{idx}) = readValue br &bytes"
                | false, _ ->
                    $"{__}let x{idx} = Cache<{canonTypeName f.PropertyType.FullName}>.Retrieve().Read(br, bytes)"
            )
        yield $"{__}{c.name}{destructFields c}"
    ]
    |> List.map (indentLine 3)
    |> String.concat "\n" }
{__}{__}{__}| _ ->
{__}{__}{__}{__}failwith "Unexpected DU case tag"
"""
