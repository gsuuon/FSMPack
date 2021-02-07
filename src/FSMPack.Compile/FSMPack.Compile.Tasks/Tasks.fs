namespace FSMPack.Compile.Tasks

open Microsoft.Build.Utilities
open Microsoft.Build.Framework

type InitializeTask() =
    inherit Task()

    override _.Execute () =
        printfn "Initializing FSMPack"
        true
