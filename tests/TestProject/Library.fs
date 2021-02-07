namespace TestProject

open System
open TestProject.Types

open FSMPack.Format
open FSMPack.Write
open FSMPack.Read

module SayItem =
    let sayFoo foo =
        sprintf "Foo num is %i" foo.num

    let sayBar (bar: Bar) =
        sprintf "Bar is: %A" bar

    let sayBaz baz =
        sprintf "Word is: %s; bar is: %A" baz.word baz.bar


module Roundtrip =
    let readFormat (format: Format<'T>) (buf: BufWriter) =
        let bufRead = ReadOnlySpan (buf.GetWritten())

        format.Read
            ( BufReader.Create()
            , bufRead )

    let myQuix = {
        baz = {
            word = "hi"
            bar = BarFoo {
                num = 2
            }
        }
        a = 1
    }

    let roundtrip (format: Format<'T>) item =
        printfn "Trying roundtrip"
        printfn "%A" item

        let buf = BufWriter.Create 0
        format.Write buf item

        let read = readFormat format buf
        printfn "%A" <| read
        item = read

    let tryRoundtrip () =
        ignore <| FSMPack.GeneratedFormats.initialize()

        roundtrip (Cache<Quix>.Retrieve()) myQuix
