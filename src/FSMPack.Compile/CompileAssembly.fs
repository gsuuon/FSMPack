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

let publishProject projectPath args =
    let startInfo = ProcessStartInfo("dotnet", "publish " + projectPath + " " + args)
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true

    let p = Process.Start startInfo
    p.WaitForExit()
    if p.ExitCode <> 0 then
        failwith
        <| sprintf "Project %s failed to publish\n%s"
            projectPath
            (p.StandardOutput.ReadToEnd())
    else
        let output = p.StandardOutput.ReadToEnd()
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
