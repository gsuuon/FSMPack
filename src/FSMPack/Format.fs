module FSMPack.Format

open System
open System.Collections.Generic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

type Format<'T> =
    abstract member Write : BufWriter -> 'T -> unit
    abstract member Read : BufReader * System.ReadOnlySpan<byte> -> 'T

module GenericFormatCache =
    type Specialized<'SpecializedType>() =
        static let mutable format : Format<'SpecializedType> option = None
        
        static member Store _format =
            format <- Some _format

        static member Retrieve () =
            format

    let Generalized = Dictionary<Type, Type>()

    let retrieveGeneralized<'SpecializedType> () =
        let genType = typedefof<'SpecializedType>

        match Generalized.TryGetValue genType with
        | true, format ->
            format
        | false, _ ->
            failwith ("missing Format type for " + string typeof<'SpecializedType>)

    let specializeAndActivate<'SpecializedType> (genFormatType: Type) =
        let specializations = typeof<'SpecializedType>.GetGenericArguments()

        let specializedFormatType = genFormatType.MakeGenericType specializations 
        let specializedFormatInstance = Activator.CreateInstance specializedFormatType :?> Format<'SpecializedType>

        specializedFormatInstance

    let retrieve<'SpecializedType> () =
        match Specialized<'SpecializedType>.Retrieve() with
        | Some f ->
            f
        | None ->
            let format =
                retrieveGeneralized<'SpecializedType>()
                |> specializeAndActivate<'SpecializedType>

            Specialized<'SpecializedType>.Store format
            format
        
let mutable _knownTypes = []

type Cache<'T>() =
    static let mutable format : Format<'T> option = None
    
    static member Store _format =
        printfn "Stored format: %A" typeof<'T>
        _knownTypes <- _knownTypes @ [typeof<'T>]

        format <- Some _format

    static member StoreGeneric genFormatType =
        printfn "Stored generic format: %A" typedefof<'T>
        _knownTypes <- _knownTypes @ [typedefof<'T>]

        GenericFormatCache.Generalized.[typedefof<'T>] <- genFormatType
        
    static member Retrieve () =
        let typ = typeof<'T>

        if typ.IsGenericType then
            GenericFormatCache.retrieve<'T> ()
        (* else if typ.IsArray then *)
        (*     let matchArrayType = typedefof<_ array> *)
                // using typedefof to match how it will be inserted
                // for array typs, typedefof and typedef return the same thing
                // right now since it's not actually a generic
            (* match GenericFormatterCache.Generalized.TryGetValue matchArrayType with *)
            (* | false, _ -> failwith "Missing array format" *)
            (* | true, formatType -> *)

        else
            match format with
            | Some f ->
                f
            | None ->
                failwith
                    <| "missing Format for " + string typ
                        + " -- known types:\n" + (_knownTypes |> List.map string |> String.concat "\n")
