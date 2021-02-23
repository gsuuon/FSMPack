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

// Formats found in FSMPack/Formats/
open FSMPack.Formats.Standard

let knownTypes = dict [
    typedefof<Map<_,_>>, typeof<IDict.FormatMap<_,_>>
    typedefof<Dictionary<_,_>>, typeof<IDict.FormatDictionary<_,_>>
    typedefof<IDictionary<_,_>>, typeof<IDict.FormatIDictionary<_,_>>

    typedefof<Tuple<_,_>>, typeof<Core.FormatFSharpTuple2<_,_>>
    typedefof<Tuple<_,_,_>>, typeof<Core.FormatFSharpTuple3<_,_,_>>
    typedefof<_ option>, typeof<Core.FormatOption<_>>

    typedefof<_ list>, typeof<Sequence.FormatFSharpList<_>>
    typedefof<_ array>, typeof<Sequence.FormatFSharpArray<_>>

    typeof<string>, typeof<Primitives.FormatString>

    typeof<int>, typeof<Primitives.FormatInt>
    typeof<int16>, typeof<Primitives.FormatInt16>
    typeof<int64>, typeof<Primitives.FormatInt64>

    typeof<byte>, typeof<Primitives.FormatByte>
    typeof<float>, typeof<Primitives.FormatFloat>
    typeof<bool>, typeof<Primitives.FormatBool>

    typeof<unit>, typeof<FSMPack.FormatUnitWorkaround.FormatUnit>
]

type CategorizedTypes =
    {
        knownTypes : Type list
        unknownTypes : Type list
        duTypes : Type list
        recordTypes : Type list
        enumTypes : Type list
    }
    static member Empty = {
        knownTypes = []
        unknownTypes = []
        recordTypes = []
        duTypes = []
        enumTypes = []
    }

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
        else if typ.IsEnum then
            { catTypes with enumTypes = typ :: catTypes.enumTypes }
        else if typ.IsArray then
            { catTypes with knownTypes = typ :: catTypes.knownTypes }
        else
            { catTypes with unknownTypes = typ :: catTypes.unknownTypes }

let discoverAllChildTypes rootTypes =
    rootTypes
    |> List.fold
        getAllSubtypesOf
        (HashSet())

/// Returns list of generalized types; ie Map<int, bool> is returned as Map<_, _>
let uniqueGeneralizedTypes types =
    types
    |> Seq.map generalize
    |> HashSet
    |> Seq.toList
    |> List.fold categorizeTypes CategorizedTypes.Empty
