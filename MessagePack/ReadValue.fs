module FSMPack.ReadValue

open Microsoft.FSharp.Core

open FSMPack.Spec

let readFormat (byt: byte) =
    let format : Format =
        LanguagePrimitives.EnumOfValue<byte, Format>(byt)

    format

let readFormatValue format header bytes =
    match format with
    | Format.PositiveFixInt
        -> // do stuff with header
        ()

    | Format.False ->
        false
        // do i just make a wrapper struct DU?

let read (bytes: byte[]) =
    // figure out format by header byte
    let format = readFormat bytes.[0]

    let value = consumeBytes format bytes.[1..]
