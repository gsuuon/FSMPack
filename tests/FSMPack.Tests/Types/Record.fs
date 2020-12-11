module FSMPack.Tests.Types.Record

open FSMPack.Format

open FSMPack.Spec
open FSMPack.Read
open FSMPack.Write

type MyInnerType = {
    C : string
}

type MyTestType = {
    A : int
    B : float
    inner : MyInnerType
}

#nowarn "0025" // incomplete pattern matches
type FormatMyInnerType() =
    interface Format<MyInnerType> with
        member _.Write bw (v: MyInnerType) =
            writeMapFormat bw 1
            writeValue bw (RawString "C")
            writeValue bw (RawString v.C)

        member _.Read (br, bytes) =
            let count = 1
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable C = Unchecked.defaultof<string>

            while items < count do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "C" ->
                        let (RawString x) = readValue br &bytes
                        C <- x
                    | _ -> failwith "Unknown key"
                | _ -> failwith "Unexpected key type"

                items <- items + 1

            {
                C = C
            }
    
type FormatMyTestType() =
    interface Format<MyTestType> with
        member _.Write bw (v: MyTestType) =
            writeMapFormat bw 3
            writeValue bw (RawString "A")
            writeValue bw (Integer v.A)
            writeValue bw (RawString "B")
            writeValue bw (FloatDouble v.B)
            writeValue bw (RawString "inner")
            Cache<MyInnerType>.Retrieve().Write bw v.inner
            
        member _.Read (br, bytes) =
            let count = 3
            let expectedCount = readMapFormatCount br &bytes

            if count <> expectedCount then
                failwith
                    ("Map has wrong count, expected " + string count
                        + " got " + string expectedCount)

            let mutable items = 0
            let mutable A = Unchecked.defaultof<int>
            let mutable B = Unchecked.defaultof<float>
            let mutable inner = Unchecked.defaultof<MyInnerType>

            while items < count do
                match readValue br &bytes with
                | RawString key ->
                    match key with
                    | "A" ->
                        let (Integer x) = readValue br &bytes
                        A <- x

                    | "B" ->
                        let (FloatDouble x) = readValue br &bytes
                        B <- x
                    | "inner" ->
                        inner <-
                            Cache<MyInnerType>.Retrieve().Read(br, bytes)

                    | _ -> failwith "Unknown key"
                | _ -> failwith "Unexpected key type"

                items <- items + 1

            {
                A = A
                B = B
                inner = inner
            }

