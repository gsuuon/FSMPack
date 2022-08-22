## Upversion

From [#1](/../../issues/1)

- [ ] Root type field which contains a stable hash of itself and all subtypes (stable relative to field types)
    - [ ] serialized as an invisible field using a custom msgpack type
    - [-] as a static field fsmpackHash? -- may not be possible, adds external data
    - [ ] doesn't alter the type
    - [ ] added during code generation to a separate type?
File: Generated.Hashes.fs
```fsharp
type Hashes = // these are the current hashes of these types
    interface FSMPackHash<Foo> with
        member _.Hash = $hash
    interface FSMPackHash<Bar> with
        member _.Hash = $hash
```
- [ ] Generated deserialize on root types check expected hash against provided and warns against mismatch
- [ ] Verifies two versions can roundtrip by comparing hashes
- [ ] Take 2 commits of an fsmpack project and generate or add to an upversion project
- [ ] Works with CI / hooks (`git work-tree .. && dotnet run -- verify ..`)

Upversion project:
* References multiple versions
* `<Aliases>` + extern alias -- F# doesn't support `extern alias` so we'll need an extra csproj
```csproj
<Reference Include="$dllnameA-$commit.dll">
    <Aliases>$commit</Aliases>
</Reference
<Reference Include="$dllnameB-$commit.dll">
    <Aliases>$commit</Aliases>
</Reference
```
* Generated placeholders for every type which doesn't match
file: `$dllnameA-$commit.fs`
```fsharp
open $commit.CSharpExternShim // or however this would work

FSMPack.Upversion.register $fooHashA $fooHashB
    { new Converts<Last.Foo, Next.Foo> with
        member _.Convert x =
            () // FIXME write conversion
    }

FSMPack.Upversion.register $barHashA $barHashB
    { new Converts<Last.Bar, Next.Bar> with
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
