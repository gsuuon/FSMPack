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
        let knownGenTypeNames = dict [
            typedefof<Map<_,_>>, "Map"
            typedefof<_ list>, "List"
            typedefof<_ option>, "Option"
        ]

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
            match knownGenTypeNames.TryGetValue (generalize typ) with
            | true, typName -> typName
            | _ ->
                if typ.FullName = null then
                    match fullnameIsFalseReason typ with
                    | ContainsGenPrmsNotTypeDef ->
                        typ.GetGenericTypeDefinition().FullName
                    | _ ->
                        failwith
                        <| sprintf
                            "Type had null FullName: %A\nReason: %A"
                                typ
                                (fullnameIsFalseReason typ)
                else
                    typ.FullName

        /// MyType`2
        let simpleName (typ: Type) =
            match knownGenTypeNames.TryGetValue (generalize typ) with
            | true, typName -> typName
            | _ -> typ.Name

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

type GeneratorNames = {
    formatType : string
    dataType : string
    dataTypeNamedArgs : string
    dataTypeAnonArgs : string
}

let getGeneratorNames (typ: Type) =
    let fullName =
        typ
        |> fullName
        |> lexName
        |> canonName

    {
        formatType =
            fullName
            |> declarableName
            |> addNamedArgs typ
            |> (+) "FMT_"

        dataType =
            fullName

        dataTypeNamedArgs =
            fullName
            |> addNamedArgs typ

        dataTypeAnonArgs =
            fullName
            |> addAnonArgs typ
    }

let getFormatTypeName (typ: Type) =
    typ
    |> fullName
    |> lexName
    |> canonName
    |> declarableName
    |> addNamedArgs typ
    |> (+) "FMT_"

let getDataTypeName (typ: Type) =
    typ
    |> fullName
    |> lexName
    |> canonName

let writeCacheFormatLine (typ: Type) (names: GeneratorNames) =
    if typ.IsGenericType then
        // Cache<Foo<_>>.StoreGeneric typedefof<FormatFoo<_>>
        $"Cache<{names.dataTypeAnonArgs}>.StoreGeneric typedefof<{names.formatType}>"

    else
        // Cache<Foo>.Store (FormatFoo :> typeof<FormatFoo>
        $"Cache<{names.dataTypeAnonArgs}>.Store ({names.formatType}() :> Format<{names.dataTypeAnonArgs}>)"
