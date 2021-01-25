module FSMPack.Format

open System
open System.Collections.Generic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

type Format<'T> =
    abstract member Write : BufWriter -> 'T -> unit
    abstract member Read : BufReader * System.ReadOnlySpan<byte> -> 'T

module GenericFormatterCache =
    let Generalized = Dictionary<Type, Type>()
        
    type Specialized<'SpecializedType>() =
        static let mutable format : Format<'SpecializedType> option = None
        
        static member Store _format =
            format <- Some _format

        static member Retrieve () =
            match format with
            | Some f ->
                f
            | None ->
                let genFormatterType : Type = Specialized<'SpecializedType>.RetrieveGeneralized()
                let specializations = typeof<'SpecializedType>.GetGenericArguments()

                let specializedFormatterType = genFormatterType.MakeGenericType specializations 
                let specializedFormatterInstance = Activator.CreateInstance specializedFormatterType :?> Format<'SpecializedType>

                Specialized<'SpecializedType>.Store specializedFormatterInstance

                specializedFormatterInstance

        static member RetrieveGeneralized () =
            let genType = typedefof<'SpecializedType>

            match Generalized.TryGetValue genType with
            | true, formatter ->
                formatter
            | false, _ ->
                failwith ("missing Formatter type for " + string typeof<'SpecializedType>)

type Cache<'T>() =
    static let mutable format : Format<'T> option = None
    
    static member Store _format =
        format <- Some _format

    static member StoreGeneric genFormatterType =
        GenericFormatterCache.Generalized.[typedefof<'T>] <- genFormatterType
        
    static member Retrieve () =
        if typeof<'T>.IsGenericType then
            GenericFormatterCache.Specialized<'T>.Retrieve ()
        else
            match format with
            | Some f ->
                f
            | None ->
                failwith ("missing Format for " + string typeof<'T>)
