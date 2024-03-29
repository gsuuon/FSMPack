namespace FSMPack.Spec

open System.Collections.Generic

type Format =
    | PositiveFixInt = 0b00000000uy
    | FixMap = 0b10000000uy
    | FixArray = 0b10010000uy
    | FixStr = 0b10100000uy
    | Nil = 0b11000000uy
    //| NeverUsed = 0b11000001uy
    | False = 0b11000010uy
    | True = 0b11000011uy
    | Bin8 = 0b11000100uy
    | Bin16 = 0b11000101uy
    | Bin32 = 0b11000110uy
    | Ext8 = 0b11000111uy
    | Ext16 = 0b11001000uy
    | Ext32 = 0b11001001uy
    | Float32 = 0b11001010uy
    | Float64 = 0b11001011uy
    | UInt8 = 0b11001100uy
    | UInt16 = 0b11001101uy
    | UInt32 = 0b11001110uy
    | UInt64 = 0b11001111uy
    | Int8 = 0b11010000uy
    | Int16 = 0b11010001uy
    | Int32 = 0b11010010uy
    | Int64 = 0b11010011uy
    | FixExt = 0b11010100uy
    | FixExt2 = 0b11010101uy
    | FixExt4 = 0b11010110uy
    | FixExt8 = 0b11010111uy
    | FixExt16 = 0b11011000uy
    | Str8 = 0b11011001uy
    | Str16 = 0b11011010uy
    | Str32 = 0b11011011uy
    | Array16 = 0b11011100uy
    | Array32 = 0b11011101uy
    | Map16 = 0b11011110uy
    | Map32 = 0b11011111uy
    | NegativeFixInt = 0b11100000uy

[<Struct>]
type Value =
    | Nil
    | Boolean of b: bool
    | Integer of i: int
    | Integer64 of i64: int64
    | UInteger of ui: uint32
    | UInteger64 of ui64: uint64
    | FloatSingle of fs: single
    | FloatDouble of fd: double
    | RawString of rs: string
    | Binary of bin: byte[]
    | ArrayCollection of arr: Value array
    | MapCollection of map: IDictionary<Value, Value>
    | Extension of ty: int * data: byte[]
    override x.ToString () =
        match x with
        | Nil -> "Nil"
        | Boolean b -> "Boolean " + string b
        | Integer i -> "Integer " + string i
        | Integer64 i64 -> "Integer64 " + string i64
        | UInteger ui -> "UInteger " + string ui
        | UInteger64 ui64 -> "UInteger64 " + string ui64
        | FloatSingle fs -> "FloatSingle " + string fs
        | FloatDouble fd -> "FloatDouble " + string fd
        | RawString rs -> "RawString " + string rs
        | Binary bin -> "Binary " + string bin
        | ArrayCollection arr -> "ArrayCollection " + string arr
        | MapCollection map -> "MapCollection " + string map
        | Extension (ty, data) -> "Extension " + string ty + ", " + string data

module Cast =
    let asFormat (byt: byte) = LanguagePrimitives.EnumOfValue<byte, Format> byt
    let asValue (x: Format) : byte = LanguagePrimitives.EnumToValue x
