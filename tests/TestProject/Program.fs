module TestProject.Run

open System

open TestProject.Types

[<EntryPoint>]
let main argv =
    let myQuix = {
        baz = {
            word = "hi"
            bar = BarFoo {
                num = 2
            }
        }
        a = 1
    }

    let tryRoundtrip () =
        ignore <| FSMPack.GeneratedFormats.initialize()

        let writtenBytes = FSMPack.GeneratedFormats.write myQuix
        let read = FSMPack.GeneratedFormats.read writtenBytes

        read = myQuix

    if tryRoundtrip () then
        0
    else
        1
