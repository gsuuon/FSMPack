module TestProject.Run

open System
open TestProject.Roundtrip

[<EntryPoint>]
let main argv =
    printfn "TestProject running"
    if tryRoundtrip () then
        printfn "Worked"
    else
        printfn "Oh no"

    0
