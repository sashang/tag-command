
open System
open System.Diagnostics
open Argu
open System.Text

type Arguments =
    | Tag of string
    | [<MainCommand; Last; ExactlyOnce; Mandatory>] Command of string list

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Tag _ -> "the tag to apply."
            | Command _ -> "the command to run."


let runCommand commandList =
    let commandString = commandList |> String.concat " "
    printfn "command: %s" commandString
    let startInfo = new ProcessStartInfo()
    startInfo.FileName  <- "/bin/bash"
    startInfo.Arguments <- "-c " + $"\"{commandString}\""
    startInfo.UseShellExecute <- false

    startInfo.RedirectStandardOutput <- true

    let proc = new Process()
    proc.EnableRaisingEvents <- true

    let driverOutput = new StringBuilder()
    proc.OutputDataReceived.AddHandler(
        DataReceivedEventHandler(
            (fun _ args ->
                driverOutput.Append(args.Data) |> ignore
                printfn "%s" args.Data
                driverOutput.AppendLine() |> ignore))
    )

    proc.StartInfo <- startInfo
    proc.Start() |> ignore
    proc.BeginOutputReadLine()

    proc.WaitForExit()
    (proc.ExitCode, driverOutput.ToString())

[<EntryPoint>]
let main argv =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<Arguments>(programName = "tag-command", errorHandler = errorHandler)
    let results = parser.ParseCommandLine argv
    let code, output = runCommand (results.GetResult Command)
    code

