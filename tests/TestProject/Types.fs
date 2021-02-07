module TestProject.Types

open FSMPack.Attribute

type Foo = {
    num : int
}

type Bar =
    | BarFoo of Foo
    | BarFloat of float
    | BarCase

type Baz = {
    word : string
    bar : Bar
}

[<FormatGeneratorRoot>]
type Quix = {
    baz : Baz
    a : int
}
