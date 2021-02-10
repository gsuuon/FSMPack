module FSMPack.Compile.Tests.Generator

open System
open System.IO
open System.Diagnostics

open System.Reflection

open Expecto

open FSMPack.Format
open FSMPack.Write
open FSMPack.Read
open FSMPack.Compile
open FSMPack.Compile.CompileAssembly

open FSMPack.Tests.Utility
open FSMPack.Tests.FormatTests

open FSMPack.Tests.Types
open FSMPack.Tests.Types.Record
open FSMPack.Tests.Types.DU
open FSMPack.Tests.Types.Mixed
open FSMPack.Tests.Types.Collection

[<AutoOpen>]
module Configuration =
    let generatedModuleName = "FSMPack.GeneratedFormats"
    let generatedOutputDirectory = "GeneratedFormats"
    let formattersOutPath =
        Path.Join (generatedOutputDirectory, "GeneratedFormats.fs")

    let outAsmName = "outasmtest"
    let outAsmPath =
        Path.Join (generatedOutputDirectory, outAsmName + ".dll")

    let assemblyReferences = [
        "System.Memory"
        "FSMPack"
        "TestCommon"
    ]

    let additionalIncludes = []

    let searchDirs = [
        "../TestCommon/bin/Debug/netstandard2.0/publish"
    ]

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
    getTypeFromAssembly asm (generatedModuleName + "+" + typeName)

let createFormatterFromAsm asm typeName =
    let searchName = generatedModuleName + "+" + typeName

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

// NOTE need to dotnet publish `TestCommon` project
// TODO add item to start publish process
[<Tests>]
let tests =
    testSequencedGroup "Generate Assembly" <| testList "Generator" [
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
                typedefof<MyGenDU<_>>

                typeof<FSharpCollectionContainer>
            ]
            |> List.fold
                AnalyzeInputAssembly.categorizeTypes
                AnalyzeInputAssembly.CategorizedTypes.Empty
            |> GenerateFormat.produceFormatsText
            |> GenerateFormat.writeText formattersOutPath

            "Formats written"
            |> Expect.isTrue (File.Exists formattersOutPath)

        testCase "fsc compiles text" <| fun _ ->
            runCompileProcess {
                files = additionalIncludes @ [formattersOutPath] 
                references = assemblyReferences
                outfile = outAsmPath
                libDirs = searchDirs
            }

            "Dll written"
            |> Expect.isTrue (File.Exists outAsmPath)

        testCase "can initialize Cache" <| fun _ ->
            let asm = Assembly.LoadFrom outAsmPath

            let startupType =
                getTypeFromAssembly
                    asm
                    ("<StartupCode$" + outAsmName +
                        ">.$" + generatedModuleName)

            Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor startupType.TypeHandle

        testList "Roundtrip" [
            testCase "Setup basic formatters" FSMPack.BasicFormats.setup

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

            testList "Format.Collection container" [
                testCase "FSharpMap" <| fun _ ->
                    "roundtrip"
                    |> roundtripFormat
                        (Cache<FSharpCollectionContainer>.Retrieve())
                        { myMap = Map.ofList [
                            0, "a"
                            1, "b" ] }
            ]

            testList "Format.Generic DU" [
                testCase "Roundtrip" <| fun _ ->
                    "Simple generic DU"
                    |> roundtripFormat
                        (Cache<MyGenDU<_>>.Retrieve())
                        (MyT "hi")
            ]
        ]
    ]
