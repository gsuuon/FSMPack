## Upversion

From [#1](/../../issues/1)

- [ ] Root type field which contains a stable hash of itself and all subtypes (stable relative to field types)
    - [ ] serialized as an invisible field using a custom msgpack type
    - [-] as a static field fsmpackHash? -- may not be possible, adds external data
    - [ ] doesn't alter the type
    - [ ] added during code generation to a separate type?

file: Generated.Hashes.fs
```fsharp
type Hashes = // these are the current hashes of these types
    interface FSMPackHash<Foo> with
        member _.Hash = $hash
    interface FSMPackHash<Bar> with
        member _.Hash = $hash
```
- [ ] Generated deserialize on root types check expected hash against provided and warns against mismatch
- [ ] Verifies each subtype of the root has the same hash
- [ ] Take 2 commits of an fsmpack project and generate or add to an upversion project
- [ ] Works with CI / hooks (`git work-tree .. && fsmpack -- verify ..`)

Upversion project:
* Tool generates or adds to the project by commit hash
    * checks out commit, builds, stores
    * compares generated hashes
* References multiple dll versions named by commits
* `<Aliases>` + extern alias -- F# doesn't support `extern alias` so we'll need an extra csproj

file: AliasShim.csproj
```csproj
<Reference Include="$dllnameA-$commit1.dll">
    <Aliases>$commit1</Aliases>
</Reference>
<Reference Include="$dllnameB-$commit1.dll">
    <Aliases>$commit1</Aliases>
</Reference>
```


* Generated placeholders for every type which doesn't match

file: `$dllnameA-$commit1-$commit2.fs`
```fsharp
FSMPack.Upversion.register $fooHashA $fooHashB
    { new Converts<$commit1.AliasShim.Foo, $commit2.AliasShim.Foo> with
        member _.Convert x =
            () // FIXME write conversion
    }

FSMPack.Upversion.register $barHashA $barHashB
    { new Converts<$commit1.AliasShim.Bar, $commit2.AliasShim.Bar> with
        member _.Convert x =
            () // FIXME write conversion
        interface Converter with // non-generic version?
            ...
    }
```

file: `Library.fs`
```fsharp
namespace FSMPack

type Upversion() =
    let graph = ..

    static member register hashA hashB converter = 
        graph.Add hashA hashB (converter :> Converter)
        
    static member convert<'T> binary : 'T =
        // get hash from binary, expect the first item to be a special type containing hash
        let originHash = readHashItem binary
        // get target hash from 'T 
        let targetHash = (Hashes :>FSMPackHash<'T>).Hash
        // get converters
        let converters = graph.Resolve originHash targetHash
        // Apply all and runtime-cast
        converters
         |> List.Fold (fun data converter -> converter.Convert data)
         :?> 'T

            
        ..
```

file: `Program.fs`
```fsharp
open MyCurrentTypes
open type FSMPack.Upversion

let x : MyCurrentFoo = convert staleBinary
```

* Library for running a binary through upversion projects, reading the version field
* Upversion dlls saved to same directory, named `<hashA>_<hashB>.dll`
* Library checks files, finds path from hash field of binary to desired type, executes conversions


## Usage

Each time a type within a root type changes, we'll generate file: `$dllnameA-$commit1-$commit2.fs` and AliasShim.csproj gets a copy of the dll -- nothing else changes. The commit hash is only used for the file name, it's not used during ser/de - just the hash of the type.
