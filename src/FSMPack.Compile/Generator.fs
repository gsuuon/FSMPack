module FSMPack.Compile.Generator
/// Generates code as strings for a type

open System
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection


let __ = "    "
let indentLine count line = String.replicate count __ + line

type RecordField =
  { name : string
    typ : Type
    get : MethodInfo
    set : MethodInfo }

let msgpackTypes = dict [
    typeof<unit>, "Nil"
    typeof<bool>, "Boolean"
    typeof<int>, "Integer"
    typeof<int64>, "Integer64"
    typeof<uint>, "UInteger"
    typeof<uint64>, "UInteger64"
    typeof<single>, "FloatSingle"
    typeof<double>, "FloatDouble"
    typeof<string>, "RawString"
    typeof<byte[]>, "Binary"
        // TODO do I need to specialize these?
    (* typeof<_ array>, "ArrayCollection" *)
    (* typeof<IDictionary<_,_>>, "MapCollection" *)
        // TODO Extension
]

let writeValueString f (typ: Type) =
    match msgpackTypes.TryGetValue f.typ with
    | true, mpType ->
        $"writeValue bw ({mpType} v.{f.name})"
    | false, _ ->
        $"Cache<{f.typ}>.Retrieve().Write bw v.{f.name}"
    
let getFields (typ: Type) =
   [ for pi in FSharpType.GetRecordFields typ do
      { name = pi.Name
        typ = pi.PropertyType
        get = pi.GetGetMethod()
        set = pi.GetSetMethod() } ]

let generateFormatRecord (typ: Type) =
    let fields = getFields typ

    $"""
type Format{typ}() =
{__}interface Format<{typ}> with
{__}{__}member _.Write bw (v: {typ}) =
{__}{__}{__}writeMapFormat bw {fields.Length}
{ [ for f in fields do
        yield $"writeValue bw (RawString \"{f.name}\")"
        yield writeValueString f typ ]
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
        yield $"let mutable {f.name} = Unchecked.defaultof<{f.typ}>" ]
    |> List.map (indentLine 3)
    |> String.concat "\n" }
{__}{__}{__}while items < count do
{__}{__}{__}{__}match readValue br &bytes with
{__}{__}{__}{__}| RawString key ->
{__}{__}{__}{__}{__}match key with
{ [ for f in fields do
        yield $"| \"{f.name}\" ->"

        match msgpackTypes.TryGetValue f.typ with
        | true, mpType -> // TODO special case for extension types
            yield $"{__}let ({mpType} x) = readValue br &bytes"
            yield $"{__}{f.name} <- x"
        | false, _ ->
            yield $"{__}{f.name} <- Cache<{f.typ}>.Retrieve().Read(br, bytes)"
        yield "| _ -> failwith \"Unknown key\""
    ]
    |> List.map (indentLine 5)
    |> String.concat "\n" }
{__}{__}{__}{__}items <- items + 1

{__}{__}{__}{{
{ [ for f in fields do
        yield $"{f.name} = {f.name}" ]
    |> List.map (indentLine 4)
    |> String.concat "\n" }
{__}{__}{__}}}
"""

let generateFormat (typ: Type) =
    if FSharpType.IsRecord typ then
        generateFormatRecord typ
    else
        "()"
