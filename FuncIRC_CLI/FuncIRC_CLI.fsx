
#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "IRCTestInfo.fsx"
#load "ConsoleHelpers.fsx"
#load "GeneralHelpers.fsx"
#load "CLIView.fsx"

namespace FuncIRC_CLI

module CLI =
    open System

    open FuncIRC.MessageParser
    open IRCTestInfo
    open ConsoleHelpers
    open GeneralHelpers
    open CLIView

    let consoleSize = {Width = 128; Height = 16}
    let consoleView = CLIView(consoleSize.Height, consoleSize.Width)

    do
        Console.Title <- "FuncIRC CLI"
        Console.Clear()
        
        let titleString = "[ FuncIRC CLI ]"
        let title = (buildString "#" consoleSize.Width)
                        .Remove(consoleSize.Width / 2 - titleString.Length / 2, titleString.Length)
                        .Insert (consoleSize.Width / 2 - titleString.Length / 2, titleString)

        consoleView.SetLine ({Content = toStringFormat title;
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, 0)

        consoleView.SetBaseForegroundColor ConsoleColor.Green
        consoleView.SetBaseBackgroundColor ConsoleColor.Black

    [<EntryPoint>]
    let main argv =
        //cprintfn ConsoleColor.Green "----- FuncIRC CLI -----"
        
        consoleView.Draw()

        //messageSplit testMessage0
        0 // return an integer exit code
