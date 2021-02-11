The FSharp compiler uses magic to handle unit types, the workaround is use the C# compiler instead.


Format<unit> is illegal. I can't write a class which implements the Format interface in F# due to this compiler error:

> The member 'Read : BufReader * System.ReadOnlySpan<byte> -> unit' is specialized with 'unit' but 'unit' can't be used as return type of an abstract method parameterized on return type. [17: typecheck]

This workaround is necessary because I need a Format<unit> for generics where a type argument could be Unit and only knowable at runtime.

https://github.com/fsharp/FSharp.Compiler.Service/blob/f73e30f367df7e4270e013732e17973c806d2389/src/fsharp/CompileOps.fs#L1349
