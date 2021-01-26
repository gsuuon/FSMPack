module FSMPack.Tests.CompileHosted

open System.IO
open FSharp.Compiler.SourceCodeServices

let directory = "GeneratedFormatters"

let assemblyReferences = [
    "./System.Memory.dll"
    "../../src/FSMPack/bin/Debug/netstandard2.0/FSMPack.dll"
]

let additionalIncludes = [
    "../FSMPack.Tests/Types/DU.fs"
    "../FSMPack.Tests/Types/Record.fs"
]

let prependToEach item xs =
    xs
    |> List.map (fun x -> [item; x])
    |> List.concat

let buildCompilerArgs filepaths references outfile =
    [
        filepaths |> prependToEach "-a"
        references |> prependToEach "-r"
        [ "-o"; outfile ]
    ]
    |> List.concat
    |> List.toArray

let compileDynAsm formattersOutPath =
    let compilerArgs =
        buildCompilerArgs
            ([formattersOutPath] @ additionalIncludes)
            assemblyReferences
            (Path.Join (directory, "GeneratedFormatters.fs") )

    let checker = FSharpChecker.Create()

    let errors, exitCode, dynAssembly = 
        checker.CompileToDynamicAssembly
            (compilerArgs, execute=None)
        |> Async.RunSynchronously

    if errors.Length > 0 then
        let msg =
            errors
            |> Array.map (fun e ->
                e.ToString()
                )
            |> String.concat "\n"

        failwith msg

    dynAssembly
