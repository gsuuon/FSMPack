namespace TestProject

open TestProject.Types

module SayItem =
    let sayFoo foo =
        sprintf "Foo num is %i" foo.num

    let sayBar (bar: Bar) =
        sprintf "Bar is: %A" bar

    let sayBaz baz =
        sprintf "Word is: %s; bar is: %A" baz.word baz.bar
