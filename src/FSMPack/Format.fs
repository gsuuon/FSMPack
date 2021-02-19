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
    type Specialized<'Specialized>() =
        static let mutable format : Format<'Specialized> option = None
        
        static member Store _format =
            format <- Some _format

        static member Retrieve () =
            format

    let Generalized = Dictionary<Type, Type>()

    let retrieveGeneralized<'Specialized> () =
        let genType = typedefof<'Specialized>

        match Generalized.TryGetValue genType with
        | true, format ->
            format
        | false, _ ->
            failwith 
            <| "missing format type for " + string typeof<'Specialized>
                + " which has a generic type def of " + string genType
                + "\nKnown generic types:\n"
                + (Generalized.Keys
                    |> Seq.map string
                    |> String.concat "\n"
                    )

    let specializations<'Specialized> () =
        typeof<'Specialized>.GetGenericArguments()

    let activateFormatSpecializations specializations (genType: Type) =
        let specializedFormatType =
            genType.MakeGenericType specializations 

        let specializedFormatInstance =
            Activator.CreateInstance
                specializedFormatType :?> Format<'Specialized>
        specializedFormatInstance

    let storeAndReturn<'Specialized> (format: Format<'Specialized>) =
        Specialized<'Specialized>.Store format
        format

    let retrieve<'Specialized> () =
        match Specialized<'Specialized>.Retrieve() with
        | Some f ->
            f
        | None ->
            retrieveGeneralized<'Specialized>()
            |> activateFormatSpecializations
                (specializations<'Specialized>())
            |> storeAndReturn
        
let mutable _knownTypes = []
let arrayGenType = typedefof<_ array>
    // Pretend this is generic, actually obj[]
    // using typedefof to match how it will be inserted
    // for array types, typedefof and typedef return the same thing

type Cache<'T>() =
    static let mutable format : Format<'T> option = None
    
    static member Store _format =
        _knownTypes <- typeof<'T> :: _knownTypes

        format <- Some _format

    static member StoreGeneric genFormatType =
        _knownTypes <- typedefof<'T> :: _knownTypes

        GenericFormatCache.Generalized.[typedefof<'T>] <- genFormatType
        
    static member Retrieve () =
        let typ = typeof<'T>

        if typ.IsGenericType then
            GenericFormatCache.retrieve<'T> ()
        else if typ.IsArray then
            match GenericFormatCache.Specialized<'T>.Retrieve() with
            | Some format -> format
            | None ->
                let arrayGenFormat =
                    GenericFormatCache.Generalized.TryGetValue
                        arrayGenType 

                match arrayGenFormat with
                | false, _ -> failwith "Missing array format"
                | true, formatType ->
                    formatType
                    |> GenericFormatCache.activateFormatSpecializations
                        [|typ.GetElementType()|]
                    |> GenericFormatCache.storeAndReturn

        else
            match format with
            | Some f ->
                f
            | None ->
                failwith
                    <| "missing Format for " + string typ
                        + " -- known types:\n" + (_knownTypes |> List.map string |> String.concat "\n")

let writeBytes<'T> value =
    let bw = BufWriter.Create 0
    Cache<'T>.Retrieve().Write bw value

    bw.GetWritten()

let readBytes<'T> bytes =
    try
        Cache<'T>.Retrieve().Read
            (BufReader.Create(), ReadOnlySpan bytes)
    
    with
    | :? MatchFailureException as ex  ->
        failwith
        <| "Match failure at " + ex.Data0 + " line: " + ex.Data1.ToString()
