module FSMPack.Compile.Tests
open Expecto

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssemblyWithCLIArgs
        [
            CLIArguments.Summary
        ] argv
