module FSMPack.Format

open System
open System.Collections.Generic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

type Format<'T> =
    abstract member Write : BufWriter -> 'T -> unit
    abstract member Read : BufReader * System.ReadOnlySpan<byte> -> 'T

type Cache<'T>() =
    static let mutable format : Format<'T> option = None
    
    static member Store _format =
        format <- Some _format
        
    static member Retrieve () =
        match format with
        | Some f ->
            f
        | None ->
            failwith ("missing Format for " + string typeof<'T>)


[<AutoOpen>]
module Generic =
    type MyGenericRecord<'T> = {
        foo : 'T
    }

    type FormatMyGenericRecord<'T>() =
        interface Format<MyGenericRecord<'T>> with
            member _.Write bw (v: MyGenericRecord<'T>) =
                writeMapFormat bw 1
                writeValue bw (RawString "foo")
                Cache<'T>.Retrieve().Write bw v.foo

            member _.Read (br, bytes) =
                let count = 1
                let expectedCount = readMapFormatCount br &bytes

                let mutable items = 0
                let mutable foo = Unchecked.defaultof<'T>

                while items < count do
                    match readValue br &bytes with
                    | RawString key ->
                        match key with
                        | "foo" ->
                            foo <- Cache<'T>.Retrieve().Read (br, bytes)
                        | _ -> failwith "Unknown key"
                    | _ -> failwith "Unexpected key type"

                    items <- items + 1

                {
                    foo = foo
                }

let isGenericDefault (typ: Type) =
    typ.IsGenericType &&
        Array.forall
            (fun t -> t = typeof<obj>)
            (typ.GetGenericArguments())

type GenericCache<'T>() =
    static let mutable formatConstructor : (unit -> Format<'T>) option = None
    static let mutable format : Format<'T> option = None

    static member Generalized (typ: Type) =
        if typ.IsGenericTypeDefinition then
            typedefof<GenericCache<_>>.MakeGenericType
                [|typ|]
            |> Some
        else
            None

    static member CallRetrieve (typ: Type) =
        typ.GetMethod("Retrieve").Invoke(null, null)

    static member CallStore format (typ: Type) =
        typ.GetMethod("Store").Invoke(null, [|format|])

    static member Store format' =
        failwith <| sprintf "Stored format for type %A" typeof<'T>
        format <- Some format'

    static member StoreGeneric format' =
        match GenericCache<_>.Generalized typedefof<'T> with
        | Some cache ->
            GenericCache<_>.CallStore format' cache
        | None ->
            failwith <| sprintf "Couldn't generalize type %A" typeof<'T>
        
    static member Retrieve () =
        match format with
        | Some f ->
            f
        | None ->
            match GenericCache<_>.Generalized typedefof<'T> with
            | Some cache -> 
                let format' = GenericCache<_>.CallRetrieve cache
                if format' <> null then
                    format' :?> Format<'T>
                else
                    failwith ("missing format for " + string typeof<'T>)
            | None ->
                failwith ("missing Format for " + string typeof<'T>)
