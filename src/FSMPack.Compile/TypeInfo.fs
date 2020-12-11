module FSMPack.Compile.TypeInfo

open System
open System.Reflection
open Microsoft.FSharp.Reflection

module Predicates =
    let isDUNullCase (typ: Type) =
        (*
            type DU =
                | DUNullCase
                | DUWithIntItem of int
         *)
        let onlyCaseTypeBound = BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.DeclaredOnly
        let items = typ.GetProperties(onlyCaseTypeBound)
        items.Length = 0

    let isNesteddInternal (typ: Type) =
        // TODO I think this ends up covering the DU null case as well
        typ.IsNestedAssembly

let getSubtypesOf (typ: Type) = [
    let elementType = typ.GetElementType()
    if not (isNull elementType) then
        (* printfn "%sSub elementType %A" indent elementType *)
        yield elementType

    // Generic parameters, closed/partial
    for genArg in typ.GetGenericArguments() do
        if not genArg.IsGenericParameter then
            (* printfn "%sSub generic argument %A" indent genArg *)
            yield genArg

    if FSharpType.IsRecord typ then
        for pi in FSharpType.GetRecordFields(typ) do
            (* printfn "%sSub record field %A" indent pi.PropertyType *)
            yield pi.PropertyType 

    if FSharpType.IsUnion typ then
        for caseInfo in FSharpType.GetUnionCases(typ) do
            for pi in caseInfo.GetFields() do
                (* printfn "%sSub union field %A" indent pi.PropertyType *)
                yield pi.PropertyType
    ]
