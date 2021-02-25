open System
open System.Diagnostics

let writeColor color (msg: string) =
    Console.ResetColor()
    Console.ForegroundColor <- color
    Console.WriteLine msg
    Console.ResetColor()

let dotnetExec (projectDirectory, op) =
    writeColor ConsoleColor.White
    <| sprintf "%s -- %s" op projectDirectory

    let startInfo = ProcessStartInfo("dotnet", op)
    startInfo.WorkingDirectory <- projectDirectory
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true

    let p = Process.Start(startInfo)
    p.WaitForExit()

    Console.ResetColor()

    if p.ExitCode = 0 then
        writeColor ConsoleColor.Green
        <| sprintf "%s -- %s success" op projectDirectory

        true
    else

        printfn "%s" <| p.StandardOutput.ReadToEnd() 

        writeColor ConsoleColor.Red
        <| sprintf "%s -- %s failure" projectDirectory op

        false

let execSequentually (projectDirs: (string * string) list) =
    projectDirs
    |> List.forall dotnetExec

execSequentually [
    "FSMPack.Tests", "run"
    "FSMPack.Compile.Tests", "run"
    "TestProject", "run" ]
