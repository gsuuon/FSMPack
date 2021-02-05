module FSMPack.Compile.Generator.Common

open System

let __ = "    "
let indentLine count line = String.replicate count __ + line

let msgpackTypes = dict [
    typeof<unit>, "Nil"
    typeof<bool>, "Boolean"
    typeof<int>, "Integer"
    typeof<int64>, "Integer64"
    typeof<uint32>, "UInteger"
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

let deriveTypeSimpleName (typ: Type) =
    (typ.Name.Split '`').[0]

let deriveTypeName (typ: Type) = 
    let typeSimpleName = deriveTypeSimpleName typ

    let genArgs = typ.GetGenericArguments()

    if genArgs.Length > 0 then
        let genArgsForTypeName =
            genArgs
            |> Array.map (fun arg -> "'" + arg.Name)
            |> String.concat ","

        $"{typeSimpleName}<{genArgsForTypeName}>"
    else
        typeSimpleName

let deriveGenericDefaultArgs (typ: Type) =
    let genArgs = typ.GetGenericArguments()

    if genArgs.Length > 0 then
        "<" +
            ( genArgs
                |> Array.map (fun _ -> "_")
                |> String.concat "," )
            + ">"
    else
        ""

let writeCacheFormatLine (typ: Type) typName =
    let isGeneric = typ.IsGenericType

    if isGeneric then
        let simpleName = deriveTypeSimpleName typ
        let genArgs = deriveGenericDefaultArgs typ

        $"Cache<{simpleName}{genArgs}>.StoreGeneric typedefof<Format{simpleName}{genArgs}>"

    else
        $"Cache<{typName}>.Store (Format{typName}() :> Format<{typName}>)"

let getTypeOpenPath (typ: Type) =
    let declaringModule = typ.DeclaringType

    if declaringModule = null then
        typ.Namespace
    else
        declaringModule.FullName

let header = """module FSMPack.GeneratedFormatters

open System

open FSMPack.Format
open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

#nowarn "0025"

"""

(* NOTE
Using the mutable _initStartupCode to kick off initialization code of the generated module w/o reflection.
Is there a better way to do this? *)
let footer = """
let mutable _initStartupCode = 0
let initialize () =
    FSMPack.BasicFormats.setup ()

    _initStartupCode
"""
