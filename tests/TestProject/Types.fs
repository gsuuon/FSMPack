module TestProject.Types

open FSMPack.Attribute

type Foo = {
    num : int
}

type Bar =
    | BarFoo of Foo
    | BarFloat of float
    | BarCase

[<FormatGeneratorRoot>]
type Baz = {
    word : string
    bar : Bar
}
