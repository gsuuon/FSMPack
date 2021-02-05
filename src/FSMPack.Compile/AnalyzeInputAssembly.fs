module FSMPack.Compile.AnalyzeInputAssembly

open System.Reflection
open System
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

let rec getAllSubtypesOf allSubtypes (typ: Type) : Type list =
    let subtypes = getSubtypesOf typ

    if subtypes.Length = 0 then
        allSubtypes @ [typ]
    else
        subtypes
        |> List.fold
            getAllSubtypesOf
            allSubtypes
        |> List.append [typ]

let discoverAllChildTypes rootTypes =
    rootTypes
    |> List.fold
        getAllSubtypesOf
        []
