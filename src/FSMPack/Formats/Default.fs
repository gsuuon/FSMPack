module FSMPack.Formats.Default

open System.Collections.Generic

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write
open FSMPack.Format

open FSMPack.Formats.Standard

#nowarn "0025"

let setup () =
    FSMPack.FormatUnitWorkaround.FormatUnit.StoreFormat()

    Cache<int>.Store (Primitives.FormatInt() :> Format<int>)
    Cache<int16>.Store (Primitives.FormatInt16() :> Format<int16>)
    Cache<float>.Store (Primitives.FormatFloat() :> Format<float>)
    Cache<bool>.Store (Primitives.FormatBool() :> Format<bool>)
    Cache<string>.Store (Primitives.FormatString() :> Format<string>)

    Cache<IDictionary<_,_>>.StoreGeneric typedefof<IDict.FormatIDictionary<_,_>>
    Cache<Dictionary<_,_>>.StoreGeneric typedefof<IDict.FormatDictionary<_,_>>
    Cache<Map<_,_>>.StoreGeneric typedefof<IDict.FormatMap<_,_>>

    Cache<System.Tuple<_,_>>.StoreGeneric typedefof<Core.FormatFSharpTuple2<_,_>>
    Cache<System.Tuple<_,_,_>>.StoreGeneric typedefof<Core.FormatFSharpTuple3<_,_,_>>

    Cache<_ option>.StoreGeneric typedefof<Core.FormatOption<_>>
    Cache<_ list>.StoreGeneric typedefof<Sequence.FormatFSharpList<_>>
    Cache<_ array>.StoreGeneric typedefof<Sequence.FormatFSharpArray<_>>
