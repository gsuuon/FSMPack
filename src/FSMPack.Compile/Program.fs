open System
open System.IO
open System.Reflection

open FSMPack.Compile.CompileAssembly
open FSMPack.Compile.GenerateFormat
open FSMPack.Compile.AnalyzeInputAssembly
open FSMPack.Compile.Generator.Common


type CompileOptions = {
    outDir : string
    outFilename : string
    references : string list
    fsmPackProjDir : string
}

let compileTypes // FIXME this path is untested
    (opts : CompileOptions)
    (categorizedTypes: CategorizedTypes)
    =

    let skipNoticeText =
        let produceCacheRetrieveCalls types =
            types
            |> List.map (fun typ ->
                typ
                |> TypeName.getFullCanonName
                |> TypeName.Transform.addAnonArgs typ
                |> sprintf "    ignore <| Cache<%s>.Retrieve()"
                )

        let produceUnitFn fnName (fnBodyLines: string list) =
            fnBodyLines
            |> String.concat "\n"
            |> (+) (sprintf "\nlet %s () =\n" fnName)
            |> fun t ->
                if fnBodyLines.Length = 0 then
                    t + "\n    ()\n"
                else
                    t + "\n"
            
        (categorizedTypes.knownTypes
        |> produceCacheRetrieveCalls
        |> produceUnitFn "verifyFormatsKnownTypes"
        )

        +

        (categorizedTypes.unknownTypes
        |> produceCacheRetrieveCalls
        |> produceUnitFn "verifyFormatsUnknownTypes"
        )

    let formatsOutpath = Path.Join(opts.outDir, opts.outFilename)

    categorizedTypes
    |> produceFormatsText
    |> fun formatsText -> formatsText + skipNoticeText
    |> writeText formatsOutpath
    printfn "FSMPack: Formats written to %s" formatsOutpath

    printfn "FSMPack: Publishing FSMPack project %s" opts.fsmPackProjDir
    let fsmpPublishPath = publishProject opts.fsmPackProjDir "-c:Release"
    printfn "FSMPack: Adding references from %s" fsmpPublishPath

    let dllOutPath = Path.Join(opts.outDir, "FSMPack.GeneratedFormats.dll")

    printfn "FSMPack: Compiling to %s" dllOutPath

    runCompileProcess {
        outfile = dllOutPath
        files = [formatsOutpath]
        references = [
            "FSMPack"
            "System.Memory"
        ] @ opts.references
        libDirs = [fsmpPublishPath]
    }


[<EntryPoint>]
let main args =
    let generatedFsFileName = "Formats.fs"

    match args.[0] with
    | "init" -> 
        printfn "FSMPack: Creating placeholder dll"

        let generatedDir = args.[1]
        let fsmpackProjDir = args.[2]

        if not <| Directory.Exists generatedDir then Directory.CreateDirectory generatedDir |> ignore

        compileTypes
          { outDir = generatedDir
            outFilename = generatedFsFileName
            references = []
            fsmPackProjDir = fsmpackProjDir }
            CategorizedTypes.Empty

    | "update" ->
        let generatedDir = args.[1]
        let fsmpackProjDir = args.[2]
        let targetDllPath = args.[3]

        printfn "FSMPack: Updating generated dll using %s" targetDllPath

        targetDllPath
        |> Assembly.LoadFrom
        |> discoverRootTypes
        |> discoverAllChildTypes
        |> compileTypes {
            outDir = generatedDir
            outFilename = generatedFsFileName
            references = [targetDllPath]
            fsmPackProjDir = fsmpackProjDir }

    | "help" ->
        printfn "init [generated dir] - create placeholder dll"
        printfn "update [generated dir] [target dll path] - update dll with generated formats from target dll"
    | _ ->
        printfn "FSMPack: Unknown command"

    0
