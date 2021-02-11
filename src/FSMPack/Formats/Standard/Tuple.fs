module FSMPack.Formats.Standard.Tuple

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write
open FSMPack.Format

#nowarn "3220"
// v.Item1
// This method or property is not normally used from F# code, use an explicit tuple pattern for deconstruction instead

type FormatFSharpTuple2<'T1,'T2>() =
    interface Format<System.Tuple<'T1,'T2>> with
        member _.Write bw (v: System.Tuple<'T1,'T2>) =
            Cache<'T1>.Retrieve().Write bw v.Item1
            Cache<'T2>.Retrieve().Write bw v.Item2

        member _.Read (br, bytes) =
            System.Tuple.Create(
                Cache<'T1>.Retrieve().Read (br, bytes),
                Cache<'T2>.Retrieve().Read (br, bytes)
            )

type FormatFSharpTuple3<'T1,'T2,'T3>() =
    interface Format<System.Tuple<'T1,'T2,'T3>> with
        member _.Write bw (v: System.Tuple<'T1,'T2,'T3>) =
            Cache<'T1>.Retrieve().Write bw v.Item1
            Cache<'T2>.Retrieve().Write bw v.Item2
            Cache<'T3>.Retrieve().Write bw v.Item3

        member _.Read (br, bytes) =
            System.Tuple.Create(
                Cache<'T1>.Retrieve().Read (br, bytes),
                Cache<'T2>.Retrieve().Read (br, bytes),
                Cache<'T3>.Retrieve().Read (br, bytes)
            )
