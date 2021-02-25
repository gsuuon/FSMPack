open System
open System.Diagnostics

let writeColor color (msg: string) =
    Console.ResetColor()
    Console.ForegroundColor <- color
    Console.WriteLine msg
    Console.ResetColor()

let runCapturedProcess (startInfo: ProcessStartInfo) = async {
    let readWaitInterval = 100
    let mutable stdOut = ""
    let mutable stdErr = ""

    startInfo.UseShellExecute <- false
    startInfo.CreateNoWindow <- true
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true

    let p = Process.Start(startInfo)

    AppDomain.CurrentDomain.ProcessExit.Add
        (fun _ ->
            if not p.HasExited then
                stdErr + p.StandardError.ReadToEnd()
                |> writeColor ConsoleColor.Red

                stdOut + p.StandardOutput.ReadToEnd() 
                |> printf "%s"

                p.Kill() )

    while not p.HasExited do
        let! newStdOut =
            p.StandardOutput.ReadToEndAsync() |> Async.AwaitTask
        stdOut <- stdOut + newStdOut

        let! newStdErr =
            p.StandardError.ReadToEndAsync() |> Async.AwaitTask
        stdErr <- stdErr + newStdErr

        do! Async.Sleep readWaitInterval

    stdOut <- stdOut + p.StandardOutput.ReadToEnd()
    stdErr <- stdErr + p.StandardError.ReadToEnd()

    return stdOut, stdErr, p.ExitCode
}

let dotnetExec (projectDirectory, op) = async {
    sprintf "%s -- %s" op projectDirectory
    |> writeColor ConsoleColor.White

    let startInfo = ProcessStartInfo("dotnet", op)
    startInfo.WorkingDirectory <- projectDirectory

    let! (stdOut, stdErr, exitCode) = runCapturedProcess startInfo

    if exitCode = 0 then
        sprintf "%s -- %s success" op projectDirectory
        |> writeColor ConsoleColor.Green

        return true
    else
        stdOut
        |> printfn "%s"

        stdErr
        |> writeColor ConsoleColor.Red

        sprintf "%s -- %s failure" op projectDirectory
        |> writeColor ConsoleColor.Red

        return false
    }

let rec execSequentually (projOps: (string * string) list) = async {
    match projOps with
    | [] ->
        return true

    | [head] ->
        return! dotnetExec head

    | head::rest ->
        match! dotnetExec head with
        | true ->
            return! execSequentually rest
        | false ->
            return false
}

execSequentually [
    "FSMPack.Tests", "run"
    "FSMPack.Compile.Tests", "run"
    "TestProject", "clean"
    "TestProject", "run"
    ]
|> Async.RunSynchronously
