module FSMPack.Compile.CompileAssembly

open System.IO

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
        [ "-o"; args.outfile ]
    ]
    |> List.concat
    |> List.toArray

let startCompileProcess args =
    File.Delete args.outfile
