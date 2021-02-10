module FSMPack.Tests.Types.Collection

open System.Collections.Generic

open FSMPack.Format

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

type FSharpCollectionContainer = {
    myMap : Map<int, string>
}
