module FSMPack.Tests.Compile.Generator

open System
open System.IO
open System.Diagnostics

open System.Reflection

open Expecto

open FSMPack.Format
open FSMPack.Write
open FSMPack.Read
open FSMPack.Compile

open FSMPack.Tests.Utility
open FSMPack.Tests.CompileHelper
open FSMPack.Tests.FormatTests

open FSMPack.Tests.Types
open FSMPack.Tests.Types.Record
open FSMPack.Tests.Types.DU
open FSMPack.Tests.Types.Mixed

let directory = "GeneratedFormatters"
let moduleName = "FSMPack.GeneratedFormatters+"
    // FIXME move this to a more appropriate place

let formattersOutPath =
    Path.Join (directory, "GeneratedFormatters.fs")

let writeFormatters formattersText =
    File.WriteAllText (formattersOutPath, formattersText)

let cacheGenFormatterTypeWithReflection<'T> formatterTyp = 
    let mi = (typeof<Cache<'T>>).GetMethod "StoreGeneric"
    ignore <| mi.Invoke (null, [| formatterTyp |])

let cacheFormatterWithReflection<'T> formatterObj = 
    let mi = (typeof<Cache<'T>>).GetMethod "Store"
    ignore <| mi.Invoke (null, [| formatterObj |])

let getTypeFromAssembly (asm: Assembly) typeName =
    let formatterTyp = asm.GetType typeName

    if formatterTyp = null then
        let types = asm.GetTypes()
        failwith <|
            "Formatter didn't exist in assembly: "
            + typeName + "\nExisting types:\n"
            + (types |> Array.map string |> String.concat "\n")

    formatterTyp

let getGenericTypeFromAsm asm typeName =
    getTypeFromAssembly asm (moduleName + typeName)

let createFormatterFromAsm asm typeName =
    let searchName = moduleName + typeName

    try
        getTypeFromAssembly asm searchName
        |> Activator.CreateInstance
    with
    | :? System.MissingMethodException as e -> 
        let typ = getTypeFromAssembly asm searchName
        raise
            <| System.AggregateException ([|
                e :> System.Exception
                System.Exception (sprintf "Found type name: %s, searched for: %s" typ.FullName searchName)
                |])

let prependText text body =
    text + "\n" + body

// NOTE need to dotnet publish `TestCommon` project
// TODO add item to start publish process
[<Tests>]
let tests =
    let outAsmName = "outasmtest.dll"

    testSequenced <| testList "Generator" [
        testCase "can publish TestCommon" <| fun _ ->
            let p = Process.Start ("dotnet", "publish ../TestCommon/TestCommon.fsproj")
            p.WaitForExit()
            
        testCase "produces formatter text file" <| fun _ ->
            File.Delete formattersOutPath

            [
                typeof<MyInnerType>
                typeof<MyTestType>
                typeof<MyInnerDU>
                typeof<MyDU>
                typedefof<MyGenericRecord<_>>

                typeof<Foo>
                typeof<Bar>
                typedefof<Baz<_>>
            ]
            |> List.map Generate.generateFormat
            |> String.concat "\n"
            |> prependText
                ( "open FSMPack.Tests.Types.Record\n"
                + "open FSMPack.Tests.Types.DU\n"
                + "open FSMPack.Tests.Types.Mixed\n" )
            |> prependText Generator.Common.header
            |> writeFormatters

            "Formatters written"
            |> Expect.isTrue (File.Exists formattersOutPath)

        testCase "fsc compiles text" <| fun _ ->
            File.Delete outAsmName

            let compilerArgs =
                buildCompilerArgs {
                    files = additionalIncludes @ [formattersOutPath] 
                    references = assemblyReferences
                    outfile = outAsmName
                    libDirs = searchDirs
                }

            let p = Process.Start ("fsc.exe", compilerArgs)

            p.WaitForExit()

            "Dll written"
            |> Expect.isTrue (File.Exists outAsmName)

        testCase "Compiled formatters can be cached" <| fun _ ->
            let asm = Assembly.LoadFrom outAsmName

            createFormatterFromAsm asm "FormatMyInnerType"
            |> cacheFormatterWithReflection<MyInnerType>
            
            createFormatterFromAsm asm "FormatMyTestType"
            |> cacheFormatterWithReflection<MyTestType>
            
            createFormatterFromAsm asm "FormatMyInnerDU"
            |> cacheFormatterWithReflection<MyInnerDU>

            createFormatterFromAsm asm "FormatMyDU"
            |> cacheFormatterWithReflection<MyDU>
            
            createFormatterFromAsm asm "FormatFoo"
            |> cacheFormatterWithReflection<Foo>
            
            createFormatterFromAsm asm "FormatBar"
            |> cacheFormatterWithReflection<Bar>
            
            getGenericTypeFromAsm asm "FormatBaz`1"
            |> cacheGenFormatterTypeWithReflection<Baz<_>>

            getGenericTypeFromAsm asm "FormatMyGenericRecord`1"
            |> cacheGenFormatterTypeWithReflection<MyGenericRecord<_>>

        testList "Roundtrip" [
            testCase "Setup basic formatters" FSMPack.BasicFormatters.setup

            TestCases.records
            TestCases.DUs
            TestCases.generics

            testCase "additional types" <| fun _ ->
                "Foo roundtrips"
                |> roundtripFormat (Cache<Foo>.Retrieve()) {
                    a = 0
                }

                "Bar roundtrips"
                |> roundtripFormat (Cache<Bar>.Retrieve()) 
                    (A { a = 0 })

                "Bar roundtrips"
                |> roundtripFormat (Cache<Bar>.Retrieve()) 
                    (B 12.3)

                "Baz roundtrips"
                |> roundtripFormat (Cache<Baz<int>>.Retrieve()) {
                    b = "hi"
                    bar = B 1.1
                    c = 3
                }
        ]
    ]
