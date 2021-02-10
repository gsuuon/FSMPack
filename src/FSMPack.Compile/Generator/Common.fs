module FSMPack.Compile.Generator.Common

open System

open FSMPack.Compile.AnalyzeInputAssembly

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

let getTypeOpenPath (typ: Type) =
    let declaringModule = typ.DeclaringType

    if declaringModule = null then
        typ.Namespace
    else
        declaringModule.FullName

[<AutoOpen>]
module private TransformTypeName  =
    let genTypeMap = typedefof<Map<_,_>>

    let isMapType (typ: Type) =
        generalize typ = genTypeMap

    /// Foo`2[[string, int]] -> Foo
    /// Bar`2 -> Bar
    /// Baz -> Baz
    let lexName (typName: string) =
        (typName.Split '`').[0]


    /// MyNamespace.MyModule+MyType`2[[string, int]]
    let fullName (typ: Type) =
        if isMapType typ then
            "Map"
        else
            typ.FullName

    /// MyType`2
    let simpleName (typ: Type) =
        if isMapType typ then
            "Map"
        else
             typ.Name

    let addArgsString (typ: Type) typName argMap =
        let args =
            typ.GetGenericArguments()
            |> Array.map argMap
            |> String.concat ","

        if args.Length > 0 then
            typName + "<" + args + ">"
        else
            typName
        
    /// myTypeNameString -> myType -> MyTypeNameString<'A,'B>
    let addNamedArgs (typ: Type) typName =
        addArgsString typ typName (fun arg -> "'" + arg.Name)

    /// myTypeNameString -> myType -> MyTypeNameString<_,_>
    let addAnonArgs (typ: Type) typName =
        addArgsString typ typName (fun _ -> "_")

    /// MyNamespace.MyModule+MyType -> MyNamespace.MyModule.MyType
    let canonName (typName: string) =
        typName.Replace ("+", ".")

module TypeName =
    // TODO These don't need readable names, better to make them 
    // more explicit and avoid namespace collisions
    let simpleWithGenArgs typ =
        typ
        |> simpleName
        |> lexName
        |> addNamedArgs typ

    let fullWithAnonArgs typ =
        typ
        |> fullName
        |> canonName
        |> lexName
        |> addAnonArgs typ

    /// If is generic parameter, then 'T else T
    let field (typ: Type) =
        if typ.IsGenericTypeParameter then
            typ
            |> simpleName
            |> (+) "'"
        else
            typ
            |> fullWithAnonArgs

let writeCacheFormatLine (typ: Type) =
    let typeName =
        typ
        |> simpleName
        |> lexName
        |> addAnonArgs typ

    if typ.IsGenericType then
        // Cache<Foo<_>>.StoreGeneric typedefof<FormatFoo<_>>
        $"Cache<{typeName}>.StoreGeneric typedefof<Format{typeName}>"

    else
        // Cache<Foo>.Store (FormatFoo :> typeof<FormatFoo>
        $"Cache<{typeName}>.Store (Format{typeName}() :> Format<{typeName}>)"

