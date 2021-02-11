module FSMPack.Compile.Generator.Record

open System
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

open FSMPack.Compile.Generator.Common

let writeValueString f =
    match msgpackTypes.TryGetValue f.typ with
    | true, mpType ->
        $"writeValue bw ({mpType} v.{f.name})"
    | false, _ ->
        $"Cache<{f.typeFullName}>.Retrieve().Write bw v.{f.name}"
    
let getFields (typ: Type) = [
    for pi in FSharpType.GetRecordFields typ do
      { name = pi.Name
        typeFullName = TypeName.field pi.PropertyType
        typ = pi.PropertyType } ]

let generateFormatRecord (typ: Type) =
    let fields = getFields typ
    let names = TypeName.getGeneratorNames typ

    let keyName =
        // Avoids collision with field names in match statement
        // TODO is there a way around this?
        let mutable key = "_k"

        let fieldNames =
            fields
            |> List.map (fun f -> f.name)
            |> HashSet

        while fieldNames.Contains key do
            key <- key + "'"

        key

    $"""type {names.formatTypeNamedArgs}() =
{__}interface Format<{names.dataTypeNamedArgs}> with
{__}{__}member _.Write bw (v: {names.dataTypeNamedArgs}) =
{__}{__}{__}writeMapFormat bw {fields.Length}
{ [ for f in fields do
        yield $"writeValue bw (RawString \"{f.name}\")"
        yield writeValueString f ]
    |> List.map (indentLine 3)
    |> String.concat "\n" }

{__}{__}member _.Read (br, bytes) =
{__}{__}{__}let count = {fields.Length}
{__}{__}{__}let expectedCount = readMapFormatCount br &bytes

{__}{__}{__}if count <> expectedCount then
{__}{__}{__}{__}failwith
{__}{__}{__}{__}{__}("Map has wrong count, expected " + string count
{__}{__}{__}{__}{__}{__}+ " got " + string expectedCount)

{__}{__}{__}let mutable items = 0
{ [ for f in fields do
        yield $"let mutable {f.name} = Unchecked.defaultof<{f.typeFullName}>" ]
    |> List.map (indentLine 3)
    |> String.concat "\n" }
{__}{__}{__}while items < count do
{__}{__}{__}{__}match readValue br &bytes with
{__}{__}{__}{__}| RawString {keyName} ->
{__}{__}{__}{__}{__}match {keyName} with
{ [ for f in fields do
        yield $"| \"{f.name}\" ->"

        match msgpackTypes.TryGetValue f.typ with
        | true, mpType -> // TODO special case for extension types
            yield $"{__}let ({mpType} x) = readValue br &bytes"
            yield $"{__}{f.name} <- x"
        | false, _ ->
            yield $"{__}{f.name} <- Cache<{f.typeFullName}>.Retrieve().Read(br, bytes)"
    ] @ [ "| _ -> failwith \"Unknown key\"" ]
    |> List.map (indentLine 5)
    |> String.concat "\n" }
{__}{__}{__}{__}items <- items + 1

{__}{__}{__}{{
{ [ for f in fields do
        yield $"{f.name} = {f.name}" ]
    |> List.map (indentLine 4)
    |> String.concat "\n" }
{__}{__}{__}}}

{writeCacheFormatLine typ names}
"""
