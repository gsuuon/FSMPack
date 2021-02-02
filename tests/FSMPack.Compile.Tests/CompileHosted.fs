module FSMPack.Tests.CompileHosted

open System.IO
open FSharp.Compiler.SourceCodeServices

open FSMPack.Compile.CompileAssembly
open FSMPack.Compile.Tests.Generator

let directory = "GeneratedFormatters"

let compileDynAsm formattersOutPath =
    let compilerArgs =
        buildCompilerArgs {
            files = [formattersOutPath] @ additionalIncludes
            references = assemblyReferences
            outfile = (Path.Join (directory, "GeneratedFormatters.fs") )
            libDirs = searchDirs
        }

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
