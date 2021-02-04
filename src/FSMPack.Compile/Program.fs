open System
open System.Reflection

open FSMPack.Compile.CompileAssembly
open FSMPack.Compile.GenerateFormat
open FSMPack.Compile.AnalyzeInputAssembly

let compileTypes types =
    produceFormattersText types
    |> writeFormatterText "Generated/Formatters.fs"

    runCompileProcess {
        outfile = "Generated/outasm.dll"
        files = ["Generated/Formatters.fs"]
        references = [typeof<FSMPack.Format.Cache<_>>.Assembly.Location]
        libDirs = []
    }
    

[<EntryPoint>]
let main args =
    match args.[0] with
    | "init" -> 
        printfn "Creating placeholder dll"
        compileTypes []

    | "update" ->
        let targetDllPath = args.[1]
        printfn "Updating generated dll using %s" targetDllPath

        targetDllPath
        |> Assembly.LoadFrom
        |> discoverRootTypes
        |> compileTypes

    | "help" ->
        printfn "init - create placeholder dll"
        printfn "update [target dll path] - update dll with generated formats from target dll"
    | _ ->
        printfn "Unknown command"

    0
