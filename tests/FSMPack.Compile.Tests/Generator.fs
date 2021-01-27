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
open FSMPack.Tests.Types
open FSMPack.Tests.Types.Record
open FSMPack.Tests.CompileHelper

let directory = "GeneratedFormatters"
let formattersOutPath =
    Path.Join (directory, "GeneratedFormatters.fs")

let writeFormatters formattersText =
    File.WriteAllText (formattersOutPath, formattersText)

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

let createFormatterFromAsm asm typeName =
    let moduleName = "FSMPack.GeneratedFormatters+"

    getTypeFromAssembly asm (moduleName + typeName)
    |> Activator.CreateInstance

// NOTE need to dotnet publish `FSMPack.Tests/Types` project
[<Tests>]
let tests =
    let outAsmName = "outasmtest.dll"

    testSequenced <| testList "Generator produces code matching format" [
        testCase "Formatters produce text" <| fun _ ->
            File.Delete formattersOutPath

            [
                typeof<MyInnerType>
                typeof<MyTestType>
            ]
            |> List.map Generate.generateFormat
            |> Generate.addFormattersFileHeader
            |> writeFormatters

            "Formatters written"
            |> Expect.isTrue (File.Exists formattersOutPath)

        testCase "fsc compiles generated formatter" <| fun _ ->
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

        testCase "Compiled formatter can roundtrip" <| fun _ ->
            let asm = Assembly.LoadFrom outAsmName

            createFormatterFromAsm asm "FormatMyInnerType"
            |> cacheFormatterWithReflection<MyInnerType>
            
            createFormatterFromAsm asm "FormatMyTestType"
            |> cacheFormatterWithReflection<MyTestType>
            
            "Simple record can roundtrip"
            |> roundtripFormat
                (Cache<MyInnerType>.Retrieve())
                {
                    C = "hi"
                }
            
            "Nested record can roundtrip"
            |> roundtripFormat
                (Cache<MyTestType>.Retrieve())
                {
                    A = 5
                    B = 1.23
                    inner = {
                        C = "hello"
                    }
                }
    ]
