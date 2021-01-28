module FSMPack.Tests.Types.Mixed

type Foo = {
    a : int
}

type Bar =
    | A of Foo
    | B of float

type Baz<'T> = {
    b : string
    bar : Bar
    c : 'T
}
