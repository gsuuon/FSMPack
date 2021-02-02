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

let deriveTypeName (typ: Type) = 
    let typeSimpleName = (typ.Name.Split '`').[0]

    let genArgs = typ.GetGenericArguments()

    if genArgs.Length > 0 then
        let genArgsForTypeName =
            genArgs
            |> Array.map (fun arg -> "'" + arg.Name)
            |> String.concat ","

        $"{typeSimpleName}<{genArgsForTypeName}>"
    else
        typeSimpleName

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
