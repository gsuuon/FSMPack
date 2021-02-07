namespace FSMPack.Compile.Tasks

open System.Diagnostics
open Microsoft.Build.Utilities
open Microsoft.Build.Framework

type InitializeTask() =
    inherit Task()

    override _.Execute () =
        printfn "Initializing FSMPack"
        let p = Process.Start ("dotnet", "run -p ../FSMPack.Compile.CLI/FSMPack.Compile.CLI.fsproj -- init")
        p.WaitForExit()
        true
