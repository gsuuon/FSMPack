open System
open System.Reflection

open FSMPack.Compile.CompileAssembly
open FSMPack.Compile.GenerateFormat
open FSMPack.Compile.AnalyzeInputAssembly

let compileTypes formatsOutpath addlRefs types =
    produceFormattersText types
    |> writeText formatsOutpath

    printfn "FSMPack: Formats written to %s" formatsOutpath

    // TODO How to include references correctly?
    // - [ ] Kick off process to publish FSMPack and add output directory as libDir
    // - [ ] Add System.Memory as a dependency to FSMPack.Compile, and get assembly.Location 
    //         from compile host process (and also FSMPack)

    runCompileProcess {
        outfile = "Generated/FSMPack.GeneratedFormats.dll"
        files = [formatsOutpath]
        references = [
            "FSMPack"
            "System.Memory"
            ] @ addlRefs
        libDirs = [
            @"C:\Users\Steven\Projects\FSMPack\src\FSMPack\bin\Debug\netstandard2.0\publish"
            ]
    }

[<EntryPoint>]
let main args =
    match args.[0] with
    | "init" -> 
        printfn "FSMPack: Creating placeholder dll"
        compileTypes "Generated/Formats.fs" [] []

    | "update" ->
        let targetDllPath = args.[1]
        printfn "FSMPack: Updating generated dll using %s" targetDllPath

        targetDllPath
        |> Assembly.LoadFrom
        |> discoverRootTypes
        |> discoverAllChildTypes
        |> compileTypes "Generated/Formats.fs" [targetDllPath]

    | "help" ->
        printfn "init - create placeholder dll"
        printfn "update [target dll path] - update dll with generated formats from target dll"
    | _ ->
        printfn "FSMPack: Unknown command"

    0
