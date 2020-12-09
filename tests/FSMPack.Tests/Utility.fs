module FSMPack.Tests.Utility

open System
open Expecto

open FSMPack.Read
open FSMPack.Write

let newBufReader () = { idx = 0 }

let newBufWriter size =
  { idx = 0
    buffer = [||]
    initialSize = size }

let expectBytesRead bytes expected =
    let readBytes = ReadOnlySpan bytes
    let actual = readValue (newBufReader()) &readBytes

    sprintf "Reads %A" expected
    |> Expect.equal actual expected 

let expectValueWrites value expected =
    let bw = newBufWriter 0

    let actual =
        writeValue bw value
        bw.GetWritten()

    sprintf "Writes %A" value
    |> Expect.equal actual expected

let roundtrip v =
    let actual =
        try
            let buf = newBufWriter 0
            writeValue buf v

            let readBytes = ReadOnlySpan (buf.GetWritten())

            readValue
                (newBufReader())
                &readBytes
        with
        | error ->
            raise <| AggregateException
                ([|
                    Exception
                        (sprintf "Roundtrip failed for %A" v)
                    error
                |] )

    sprintf "Roundtrips %A" v
    |> Expect.equal actual v

let byteToString (byt: byte) =
    Convert.ToString(byt, 2).PadLeft(8, '0')
