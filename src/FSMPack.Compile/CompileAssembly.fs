module FSMPack.Compile.CompileAssembly

open System.IO

type CompilerArgs = {
    files : string list
    references : string list
    outfile : string
    libDirs : string list
}

let startCompileProcess args =
    File.Delete args.outfile
    
