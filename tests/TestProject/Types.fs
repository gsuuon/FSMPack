module TestProject.Types

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
