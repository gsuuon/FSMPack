module FSMPack.Tests.Main

open Expecto

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssemblyWithCLIArgs
        [ (* CLIArguments.Summary *) ] argv
