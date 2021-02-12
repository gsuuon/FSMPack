open System
open System.Diagnostics

let writeColor color (msg: string) =
    Console.ResetColor()
    Console.ForegroundColor <- color
    Console.WriteLine msg
    Console.ResetColor()

let runProject projectDirectory =
    writeColor ConsoleColor.White
    <| sprintf "%s -- running" projectDirectory

    let startInfo = ProcessStartInfo("dotnet", "run")
    startInfo.WorkingDirectory <- projectDirectory
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true

    let p = Process.Start(startInfo)
    p.WaitForExit()

    Console.ResetColor()

    if p.ExitCode = 0 then
        writeColor ConsoleColor.Green
        <| sprintf "%s -- run success" projectDirectory

        true
    else

        printfn "%s" <| p.StandardOutput.ReadToEnd() 

        writeColor ConsoleColor.Red
        <| sprintf "%s -- run failure" projectDirectory

        false


let runSequentially (projectDirs: string list) =
    projectDirs
    |> List.forall runProject

runSequentially [
    "FSMPack.Tests"
    "FSMPack.Compile.Tests" ]
