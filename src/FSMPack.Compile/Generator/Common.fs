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

module TypeName =
    module Transform =
        let genTypeMap = typedefof<Map<_,_>>

        let isMapType (typ: Type) =
            generalize typ = genTypeMap

        /// Foo`2[[string, int]] -> Foo
        /// Bar`2 -> Bar
        /// Baz -> Baz
        let lexName (typName: string) =
            (typName.Split '`').[0]

        type FullNameNullReason =
            | GenericTypeParameter
            | ContainsGenPrmsNotTypeDef
            | Unknown

        let fullnameIsFalseReason (typ: Type) =
            if typ.IsGenericTypeParameter then
                GenericTypeParameter
            else if typ.ContainsGenericParameters && not typ.IsGenericTypeDefinition then
                ContainsGenPrmsNotTypeDef
            else
                Unknown

        /// MyNamespace.MyModule+MyType`2[[string, int]]
        let fullName (typ: Type) =
            // fullname is null if typ:
            // - is a generic type parameter
            // - represents an array type, pointer type, or byref type based on a generic type parameter
            // - type contains generic type parameters, but is not a generic type definition
            //       - ie, ContainsGenericParameters = true; IsGenericTypeDefinition = false;
            // 
            if isMapType typ then
                "Map"
            else
                if typ.FullName = null then
                    match fullnameIsFalseReason typ with
                    | ContainsGenPrmsNotTypeDef ->
                        typ.GetGenericTypeDefinition().FullName
                    | _ ->
                        failwith <| sprintf "Type had null FullName: %A\nReason: %A" typ (fullnameIsFalseReason typ)
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

        let declarableName (canonTypeName: string) =
            canonTypeName.Replace (".", "_")

    open Transform

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

open TypeName.Transform

let writeCacheFormatLine (typ: Type) fmtTypeName =
    let typeName =
        typ
        |> fullName
        |> canonName
        |> lexName
        |> addAnonArgs typ

    if typ.IsGenericType then
        // Cache<Foo<_>>.StoreGeneric typedefof<FormatFoo<_>>
        $"Cache<{typeName}>.StoreGeneric typedefof<{fmtTypeName}>"

    else
        // Cache<Foo>.Store (FormatFoo :> typeof<FormatFoo>
        $"Cache<{typeName}>.Store ({fmtTypeName}() :> Format<{typeName}>)"

let getTypeOpenPath (typ: Type) =
    let declaringModule = typ.DeclaringType

    if declaringModule = null then
        typ.Namespace
    else
        declaringModule.FullName
        |> canonName

let formatTypeName (typ: Type) =
    typ
    |> fullName
    |> lexName
    |> canonName
    |> declarableName
    |> addNamedArgs typ
    |> (+) "FMT_"
