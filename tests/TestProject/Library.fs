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

    let myBaz = {
        word = "hi"
        bar = BarFoo {
            num = 2
        }
    }

    let tryRoundtrip () =
        ignore <| FSMPack.GeneratedFormatters.initialize()
        printfn "Trying roundtrip"
        printfn "%s" <| SayItem.sayBaz myBaz
        let buf = BufWriter.Create 0

        let format = Cache<Baz>.Retrieve()
        format.Write buf myBaz

        let read = readFormat format buf
        printfn "%s" <| SayItem.sayBaz read
        myBaz = read
