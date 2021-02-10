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

let knownTypes = HashSet [
        typedefof<Map<_,_>>
        typedefof<_ list>
        typedefof<_ option>
        typedefof<_ array>
    ]

/// setB - setA
let exclude (setA: 'a HashSet) (setB: 'a HashSet) =
    setB.ExceptWith setA
    setB

let discoverAllChildTypes rootTypes =
    rootTypes
    |> List.fold
        getAllSubtypesOf
        (HashSet())
    |> Seq.map generalize
    |> HashSet
    |> exclude knownTypes
    |> Seq.toList
