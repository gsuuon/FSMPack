module FSMPack.Tests.CompileHelper

let assemblyReferences = [
    "System.Memory"
    "FSMPack"
    "Types"
]

let additionalIncludes = []

let searchDirs = [
    "../FSMPack.Tests/Types/bin/Debug/netstandard2.0/publish"
]

type CompilerArgs = {
    files : string list
    references : string list
    outfile : string
    libDirs : string list
}

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
