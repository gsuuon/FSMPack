module FSMPack.Compile.CompileAssembly

open System
open System.IO
open System.Reflection
open System.Diagnostics
open System.Collections.Generic
open System.Text.RegularExpressions

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
        "--tailcalls-"
            // .tail instruction in release breaks writeValue call in Unity. Also:
            // https://github.com/dotnet/fsharp/issues/6329
            // - Tail opcode being emitted for normal methods, destroys JIT optimizations
        ]
    |> List.toArray

let resolveReferencesFromAssembly (asm: Assembly) (references: string list) =
    let refsToResolve = HashSet references

    let resolvedReferenceLocations =
        asm.GetReferencedAssemblies()
        |> Array.choose (fun asmName ->
            let simpleName = asmName.Name

            if refsToResolve.Remove simpleName then
                Some (Assembly.Load(asmName).Location)
            else
                None
            )

    if refsToResolve.Count > 0 then
        for unresolved in refsToResolve do
            printfn "Failed to resolve reference from %A: %A"
                asm.FullName
                unresolved

    resolvedReferenceLocations

let runCompileProcess args =
    File.Delete args.outfile

    let compilerArgs = buildCompilerArgs args

    let p = Process.Start ("fsc.exe", compilerArgs)

    p.WaitForExit()

let dotnetExec op args failMsg =
    let startInfo = ProcessStartInfo("dotnet", op + " " + args)

    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true

    let p = Process.Start startInfo
    p.WaitForExit()
    if p.ExitCode <> 0 then
        failwith
        <| sprintf "%s\n%s"
            failMsg
            (p.StandardOutput.ReadToEnd())
    else
        p.StandardOutput.ReadToEnd()

let publishProject projectPath args =
    let output =
        dotnetExec "publish"
        <| (projectPath + " " + args)
        <| sprintf "Project %s failed to publish"
            projectPath

    let publishDirOpt =
        output.Split("\n")
        |> Array.tryPick (fun line ->
            let m = Regex.Match(line, ".+ -> (.+publish.)")

            if m.Success then
                Some m.Groups.[1].Value
            else
                None
            )

    match publishDirOpt with
    | Some dir -> dir
    | None ->
        failwith
        <| sprintf
            "Failed to match publish directory from output:\n%s"
            output
