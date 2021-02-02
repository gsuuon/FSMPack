module FSMPack.Compile.Tests.Run
open Expecto

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssemblyWithCLIArgs
        [
            CLIArguments.Summary
        ] argv
