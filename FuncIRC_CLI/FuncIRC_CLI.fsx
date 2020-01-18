
#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "IRCTestInfo.fsx"
#load "ConsoleHelpers.fsx"
#load "GeneralHelpers.fsx"
#load "CLIView.fsx"
#load "Application.fsx"

namespace FuncIRC_CLI

module CLI =
    open System

    open Application
    open ConsoleHelpers
    open CLIView
    open FuncIRC.MessageParser
    open GeneralHelpers
    open IRCTestInfo

    let consoleSize = {Width = 128; Height = 16}
    let consoleView = CLIView(consoleSize.Height, consoleSize.Width)

    let defaultLine = (buildString " " consoleSize.Width).Remove(consoleSize.Width, 1)
                                                         .Remove(0, 1)
                                                         .Insert(0, "|")
                                                         .Insert(consoleSize.Width, "|")

    let topLine = (buildString "¨" consoleSize.Width)

    let app = Application (consoleView)

    let printState stateString = 
        let state = placeOnString (defaultLine, stateString, 20)
        consoleView.SetLine ({Content = toStringFormat state;
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, 10)

    let applicationStateHandler state =
        printState state
        
        match state with
        | "Quit" -> app.Stop()
        | _ -> ()

    do
        Console.Title <- "FuncIRC CLI"
        Console.Clear()

        app.SetStateListener applicationStateHandler

        let titleString = "[ FuncIRC CLI ]"
        let title = centerOnString (defaultLine, titleString)

        let usernameString = "[ Username: ________________________________ ]"
        let username = placeOnString (defaultLine, usernameString, 20)

        consoleView.SetLine ({Content = toStringFormat title;
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, 0)

        consoleView.SetLine ({Content = toStringFormat (buildString "=" consoleSize.Width);
                              ForegroundColor = ConsoleColor.Red;
                              BackgroundColor = ConsoleColor.Black}, 1)

        consoleView.SetLine ({Content = toStringFormat username;
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, 4)

        consoleView.SetLine ({Content = toStringFormat topLine;
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, consoleSize.Height)

        consoleView.SetBaseForegroundColor ConsoleColor.Green
        consoleView.SetBaseBackgroundColor ConsoleColor.Black

    [<EntryPoint>]
    let main argv =
        cprintfn ConsoleColor.Red ConsoleColor.Blue "----- FuncIRC CLI -----"
        
        (app.Run())

        //messageSplit testMessage0
        0 // return an integer exit code
