module FSMPack.Format

open System
open System.Collections.Generic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

type Format<'T> =
    abstract member Write : BufWriter -> 'T -> unit
    abstract member Read : BufReader * System.ReadOnlySpan<byte> -> 'T

type MyInnerType = {
    C : string
}

type MyTestType = {
    A : int
    B : float
    inner : MyInnerType
}

type Cache<'T>() =
    static let mutable format : Format<'T> option = None
    
    static member Store _format =
        format <- Some _format
        
    static member Retrieve () =
        match format with
        | Some f -> f
        | None ->
            failwith ("missing Format for " + string typeof<'T>)

#nowarn "0025"
type FormatMyInnerType() =
    interface Format<MyInnerType> with
        member _.Write bw (v: MyInnerType) =
            writeMapFormat bw 1
            writeString bw "C"
            writeValue bw (RawString v.C)

        member _.Read (br, bytes) =
            let size = 1
            let mutable items = 0
            let mutable C = Unchecked.defaultof<string>

            while items < size do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "C" ->
                        let (RawString x) = readValue br &bytes
                        C <- x
                    | _ -> failwith "Unknown key"
                | _ -> failwith "Unexpected key type"

                items <- items + 1

            {
                C = C
            }
    
type FormatMyTestType() =
    interface Format<MyTestType> with
        member _.Write bw (v: MyTestType) =
            writeMapFormat bw 2
            writeString bw "A"
            writeValue bw (Integer v.A)
            writeString bw "B"
            writeValue bw (FloatDouble v.B)
            writeString bw "inner"
            Cache<MyInnerType>.Retrieve().Write bw v.inner
            
        member _.Read (br, bytes) =
            let size = 2
            let mutable items = 0
            let mutable A = Unchecked.defaultof<int>
            let mutable B = Unchecked.defaultof<float>
            let mutable inner = Unchecked.defaultof<MyInnerType>

            while items < size do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "A" ->
                        let (Integer x) = readValue br &bytes
                        A <- x

                    | "B" ->
                        let (FloatDouble x) = readValue br &bytes
                        B <- x
                    | "inner" ->
                        inner <-
                            Cache<MyInnerType>.Retrieve().Read(br, bytes)

                    | _ -> failwith "Unknown key"
                | _ -> failwith "Unexpected key type"

                items <- items + 1

            {
                A = A
                B = B
                inner = inner
            }
