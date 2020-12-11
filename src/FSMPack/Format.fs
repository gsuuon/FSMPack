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
