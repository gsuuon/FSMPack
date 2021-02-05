module TestProject.Run

open System
open TestProject.Roundtrip

[<EntryPoint>]
let main argv =
    if tryRoundtrip () then
        printfn "Worked"
    else
        printfn "Oh no"

    0
