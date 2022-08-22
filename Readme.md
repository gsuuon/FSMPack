## Upversion

Copied from #1

- [ ] Root type field which contains a stable hash of itself and all subtypes (stable relative to field types)
- [ ] Generated deserialize on root types check expected hash against provided and warns against mismatch
- [ ] Tool verifies two versions can roundtrip
- [ ] Tool takes 2 versions of an FSMPack project and generates an upversion project
- [ ] Tool works with CI / hooks (`git work-tree .. && fsmpack verify ..`)
- [ ] Seperate helper project which references upversion projects

Upversion project:
* References two versions of FSMPack project (or dlls, TBD)
* `<Aliases>` + extern alias -- F# doesn't support `extern alias` so we'll need an extra csproj
* Generated placeholders for every type which doesn't match
```fsharp
namespace <hashA>_to_<hashB>
open CSharpExternShim

type Converter =
  static member convert (data: Last.RelevantType) : Next.RelevantType = ()
```

Helper project:
* Library for running a binary through upversion projects, reading the version field
* Upversion dlls saved to same directory, named `<hashA>_<hashB>.dll`
* Library checks files, finds path from hash field of binary to desired type, executes conversions
