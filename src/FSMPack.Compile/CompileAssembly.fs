module FSMPack.Compile.CompileAssembly

open System.IO
open System.Diagnostics

type CompilerArgs = {
    files : string list
    references : string list
    outfile : string
    libDirs : string list
}

[<AutoOpen>]
module Helpers =
    let prependToEach item xs =
        xs
        |> List.map (fun x -> [item; x])
        |> List.concat

    let combineWithEach item xs =
        xs
        |> List.map (fun x -> item + x)

let buildCompilerArgs args =
    [
        args.files |> prependToEach "-a"
        args.references |> prependToEach "-r"
        args.libDirs |> combineWithEach "--lib:"
    ]
    |> List.concat
    |> List.append [
        "-o"; args.outfile 
        "--nocopyfsharpcore"
        ]
    |> List.toArray

let runCompileProcess args =
    File.Delete args.outfile

    let compilerArgs = buildCompilerArgs args

    let p = Process.Start ("fsc.exe", compilerArgs)

    p.WaitForExit()
