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

type Field =
  { name : string
    typeFullName : string
    typ : Type }

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
            | ArrayWithGenPrms
            | Unknown

        let fullnameIsNullReason (typ: Type) =
            if typ.IsGenericTypeParameter then
                GenericTypeParameter
            else if typ.ContainsGenericParameters && typ.IsGenericType
                    && not typ.IsGenericTypeDefinition then
                ContainsGenPrmsNotTypeDef
            else if typ.ContainsGenericParameters && typ.IsArray then
                ArrayWithGenPrms
            else
                Unknown

        // if a type and a module share a name, the module will be implicitly
        // suffixed with 'Module', e.g
        // ```
        // type Foo = ..
        // module Foo = ...
        // ```
        // will generate Foo and FooModule types
        let stripModuleSuffix (name: string) =
            name.Split(".")
            |> Array.map
                (fun segment ->
                    let matched = Text.RegularExpressions.Regex.Match(segment, "(.+)Module$")
                    if matched.Success then
                        matched.Groups.[1].Value
                    else
                        segment
                )
            |> String.concat "."

        /// Microsoft.Collections.FSharp`2[[string, int]]
        let fullCommonName (typ: Type) =
            if typ.FullName = null then
                match fullnameIsNullReason typ with
                | ContainsGenPrmsNotTypeDef ->
                    typ.GetGenericTypeDefinition().FullName
                | ArrayWithGenPrms ->
                    "'" + typ.Name
                | _ ->
                    failwith
                    <| sprintf
                        "Type had null FullName: %A\nReason: %A"
                            typ
                            (fullnameIsNullReason typ)
            else
                typ.FullName

        /// MyNamespace.MyModule+MyType`2[[string, int]]
        let fullName (typ: Type) =
            match knownGenTypeNames.TryGetValue (generalize typ) with
            | true, typName -> typName
            | _ -> fullCommonName typ

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

    type GeneratorNames = {
        formatTypeNamedArgs : string
        formatTypeAnonArgs : string
        dataType : string
        dataTypeNamedArgs : string
        dataTypeAnonArgs : string
    }

    let getFullCanonName (typ: Type) =
        typ
        |> fullName
        |> lexName
        |> canonName
        |> stripModuleSuffix

    let getFullCommonName (typ: Type) =
        typ
        |> fullCommonName
        |> lexName
        |> canonName
        |> stripModuleSuffix

    let asFormatTypeName name =
        name
        |> declarableName
        |> (+) "FMT_"

    let getGeneratorNames (typ: Type) =
        let fullName = getFullCanonName typ
        let formatTypeName = fullName |> asFormatTypeName

        {
            formatTypeNamedArgs =
                formatTypeName
                |> addNamedArgs typ

            formatTypeAnonArgs =
                formatTypeName
                |> addAnonArgs typ

            dataType =
                fullName

            dataTypeNamedArgs =
                fullName
                |> addNamedArgs typ

            dataTypeAnonArgs =
                fullName
                |> addAnonArgs typ
        }

    /// If typ is generic parameter, then 'T else T
    let field (typ: Type) =
        if typ.IsGenericTypeParameter then
            typ
            |> simpleName
            |> (+) "'"
        else
            typ
            |> getFullCanonName
            |> addAnonArgs typ

let writeCacheFormatLine (typ: Type) (names: TypeName.GeneratorNames) =
    if typ.IsGenericType then
        // Cache<Foo<_>>.StoreGeneric typedefof<FormatFoo<_>>
        $"Cache<{names.dataTypeAnonArgs}>.StoreGeneric typedefof<{names.formatTypeAnonArgs}>"

    else
        // Cache<Foo>.Store (FormatFoo :> typeof<FormatFoo>
        $"Cache<{names.dataTypeAnonArgs}>.Store ({names.formatTypeNamedArgs}() :> Format<{names.dataTypeAnonArgs}>)"

let getWriteFieldCall (field: Field) valueText =
    match msgpackTypes.TryGetValue field.typ with
    | true, mpType ->
        $"writeValue bw ({mpType} {valueText})"
    | _ ->
        $"Cache<{field.typeFullName}>.Retrieve().Write bw {valueText}"

let getReadFieldCall (field: Field) assignVarText =
    match msgpackTypes.TryGetValue field.typ with
    | true, mpType ->
        $"let ({mpType} {assignVarText}) = readValue br &bytes"
    | _ ->
        $"let {assignVarText} = Cache<{field.typeFullName}>.Retrieve().Read(br, bytes)"
