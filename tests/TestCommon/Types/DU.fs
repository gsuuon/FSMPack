module FSMPack.Tests.Types.DU

open Microsoft.FSharp.Core
open Microsoft.FSharp.Reflection

open FSMPack.Format
open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

type MyInnerDU =
    | A
    | B of int

type MyDU =
    | C of string * float
    | D of MyInnerDU
    | E
    (* | F of int * bool *)
        // depends on tuple

type MyGenDU<'T> =
    | MyT of 'T
    | Foo

// Array size 1 + max fields count
// 1 byte tag PosFix
// fields in order

#nowarn "0025"
type FormatMyInnerDU() =
    interface Format<MyInnerDU> with
        member _.Write bw (v: MyInnerDU) =
            match v with
            | A ->
                writeArrayFormat bw 1
                writeValue bw (Integer 0)
            | B x0 ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                writeValue bw (Integer x0)

        member _.Read (br, bytes) =
            let count = readArrayFormatCount br &bytes
                // count seems useless here, but we need to consume the bytes
            match readValue br &bytes with
            | Integer 0 ->
                A
            | Integer 1 ->
                let (Integer x) = readValue br &bytes
                B x
            | _ ->
                failwith "Unexpected DU case tag"
                    // Can probably make this string a global constant

type FormatMyDU() =
    interface Format<MyDU> with
        member _.Write bw (v: MyDU) =
            match v with
            | C (x0, x1) ->
                writeArrayFormat bw 3
                writeValue bw (Integer 0)
                writeValue bw (RawString x0)
                writeValue bw (FloatDouble x1)
            | D (x0) ->
                writeArrayFormat bw 2
                writeValue bw (Integer 1)
                Cache<MyInnerDU>.Retrieve().Write bw x0
            | E ->
                writeArrayFormat bw 1
                writeValue bw (Integer 2)

        member _.Read (br, bytes) =
            let count = readArrayFormatCount br &bytes

            match readValue br &bytes with
            | Integer 0 ->
                let (RawString x) = readValue br &bytes
                let (FloatDouble y) = readValue br &bytes
                C (x, y)

            | Integer 1 ->
                let x = Cache<MyInnerDU>.Retrieve().Read(br, bytes)
                D x

            | Integer 2 ->
                E

            | _ ->
                failwith "Unexpected DU case tag"
