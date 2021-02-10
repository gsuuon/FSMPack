module FSMPack.Compile.AnalyzeInputAssembly

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Reflection

open FSMPack.Attribute

let discoverRootTypes (asm: Assembly) =
    asm.GetTypes()
    |> Array.filter (fun typ ->

        typ.GetCustomAttributes()
        |> Seq.exists (fun attr ->

            attr :? FormatGeneratorRootAttribute
            ) )

    |> Array.toList

let getSubtypesOf (typ: Type) =
    [
        let elementType = typ.GetElementType()
        if not (isNull elementType) then
            yield elementType

        // Generic parameters, closed/partial
        for genArg in typ.GetGenericArguments() do
            if not genArg.IsGenericParameter then
                yield genArg

        if FSharpType.IsRecord typ then
            for pi in FSharpType.GetRecordFields(typ) do
                yield pi.PropertyType 

        if FSharpType.IsUnion typ then
            for caseInfo in FSharpType.GetUnionCases(typ) do
                for pi in caseInfo.GetFields() do
                    yield pi.PropertyType
    ]

let rec getAllSubtypesOf (allSubtypes: HashSet<Type>) (typ: Type) : HashSet<Type> =
    if allSubtypes.Contains typ then allSubtypes else

    ignore <| allSubtypes.Add typ

    getSubtypesOf typ
    |> List.fold
        getAllSubtypesOf
        allSubtypes

let generalize (typ: Type) = 
    if typ.IsGenericType then
        typ.GetGenericTypeDefinition()
    else
        typ

// Formats found in FSMPack/BasicFormats.fs
let knownTypes = HashSet [
    typedefof<Map<_,_>>
    typedefof<Dictionary<_,_>>
    typedefof<IDictionary<_,_>>

    typedefof<_ list> // TODO
    typedefof<_ option> // TODO
    typedefof<_ array> // TODO

    typeof<string>
    typeof<int>
    typeof<float>
]

type CategorizedTypes =
    {
        knownTypes : Type list
        unknownTypes : Type list
        duTypes : Type list
        recordTypes: Type list
    }
    static member Empty = {
        knownTypes = []
        unknownTypes = []
        recordTypes = []
        duTypes = [] }

let categorizeTypes (catTypes: CategorizedTypes) (typ: Type) =
    let matchType = 
        if typ.IsGenericType then
            typ.GetGenericTypeDefinition()
        else
            typ

    match knownTypes.TryGetValue matchType with
    | true, _ ->
        { catTypes with knownTypes = typ :: catTypes.knownTypes }
    | _ ->
        if FSharpType.IsRecord typ then
            { catTypes with recordTypes = typ :: catTypes.recordTypes }
        else if FSharpType.IsUnion typ then
            { catTypes with duTypes = typ :: catTypes.duTypes }
        else
            { catTypes with unknownTypes = typ :: catTypes.unknownTypes }

let discoverAllChildTypes rootTypes =
    rootTypes
    |> List.fold
        getAllSubtypesOf
        (HashSet())
    |> Seq.map generalize
    |> HashSet
    |> Seq.toList
    |> List.fold categorizeTypes CategorizedTypes.Empty
