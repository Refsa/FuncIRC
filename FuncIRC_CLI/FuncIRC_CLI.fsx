
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

    // View
    let consoleView = CLIView(consoleSize.Height, consoleSize.Width)

    let defaultColor = CLIColor (ConsoleColor.Green, ConsoleColor.Black)
    let titleColor = CLIColor (ConsoleColor.Black, ConsoleColor.White)

    let titleString = "[ FuncIRC CLI ]"
    let titleElement = Label(titleString, CLIPosition(consoleSize.Width / 2 - titleString.Length / 2, 0), titleColor)

    let usernameString = "[ Username: ________________________________ ]"
    let usernameElement = Label(usernameString, CLIPosition(20, 4), defaultColor)

    let defaultLine = (buildString " " consoleSize.Width).Remove(consoleSize.Width, 1)
                                                         .Remove(0, 1)
                                                         .Insert(0, "|")
                                                         .Insert(consoleSize.Width, "|")

    let topLine = (buildString "¨" consoleSize.Width)

    // Update
    let app = Application (consoleView)

    let printInputStateLine stateLine = 
        let state = placeOnString (defaultLine, stateLine, 20)
        consoleView.SetLine ({Content = state;
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, 10)

    let applicationStateHandler state =
        printInputStateLine (state.Line + " - Key: " + state.Key.ToString())

        match state.Line with
        | "Quit" -> app.Stop()
        | _ -> ()

    do
        Console.Title <- "FuncIRC CLI"
        Console.Clear()

        app.SetStateListener applicationStateHandler


        consoleView.SetElement (titleElement)
        consoleView.SetElement (usernameElement)

        consoleView.SetLine ({Content = (buildString "=" consoleSize.Width);
                              ForegroundColor = ConsoleColor.Red;
                              BackgroundColor = ConsoleColor.Black}, 1)

        consoleView.SetLine ({Content = topLine;
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
