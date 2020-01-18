
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

    // Model
    type Navigation =
        {
            Focused: CLIElement
            Index: int
        }

    let mutable navigation: Navigation option = None
    let mutable navigationElements: CLIElement list = []

    // View
    let consoleView = CLIView(consoleSize.Height, consoleSize.Width)

    let defaultColor = CLIColor (ConsoleColor.Green, ConsoleColor.Black)
    let inverseColor = CLIColor (ConsoleColor.Black, ConsoleColor.White)

    let titleString = "---~~~~{### FuncIRC CLI ###}~~~~---"
    let titleElement = Label(titleString, CLIPosition(consoleSize.Width / 2 - titleString.Length / 2, 0), inverseColor)

    let usernameString = "[ Username: $t ]"
    let usernameElement = TextField(usernameString, CLIPosition(20, 4), defaultColor)

    let passwordString = "[ Password: $t ]"
    let passwordElement = TextField(passwordString, CLIPosition(20, 5), defaultColor)

    let exitString = "Exit"
    let exitElement = Button(exitString, CLIPosition(5, consoleSize.Height - 1), defaultColor)

    let defaultLine = (buildString " " consoleSize.Width).Remove(consoleSize.Width, 1)
                                                         .Remove(0, 1)
                                                         .Insert(0, "|")
                                                         .Insert(consoleSize.Width, "|")

    let topLine = (buildString "¨" consoleSize.Width)

    // Update
    let app = Application (consoleView)

    let setFocusedNavigationElement elem index =
        if navigation.IsSome then
            navigation.Value.Focused.SetColor defaultColor

        navigation <- Some {Focused = elem; Index = index}
        navigation.Value.Focused.SetColor inverseColor

    let navigate dir =
        if navigationElements.Length = 0 then ()

        match navigation with
        | None -> 
            setFocusedNavigationElement navigationElements.[0] 0
        | Some nav -> 
            match dir with
            | ConsoleKey.DownArrow ->
                let navElemsEnd = navigationElements.Length - 1
                let index = nav.Index + 1
                index |> fun x -> if x > navElemsEnd then 0 else index
            | ConsoleKey.UpArrow -> 
                let index = nav.Index - 1
                index |> fun x -> if x < 0 then navigationElements.Length - 1 else index
            | _ -> 0
            |> fun i -> 
                setFocusedNavigationElement navigationElements.[i] i

    let printInputStateLine stateLine = 
        let state = placeOnString (defaultLine, stateLine, 20)
        consoleView.SetLine ({Content = state;
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, 10)

    let applicationStateHandler state =
        printInputStateLine (state.Line + " - Key: " + state.Key.ToString())

        match navigation with
        | Some navigation -> 
            navigation.Focused.SetContent state.Line
        | None -> ()

        match state.Key with
        | ConsoleKey.Enter when state.Line = "Quit" -> app.Stop()
        | ConsoleKey.Enter -> 
            match navigation with
            | Some nav ->
                nav.Focused.Execute()
            | None -> ()
        | IsNavigationInput ck -> navigate ck
        | _ -> ()

    do
        Console.Title <- "FuncIRC CLI"
        Console.Clear()

        app.SetStateListener applicationStateHandler
        //navigation <- Some {Focused = (usernameElement :> CLIElement); Index = 0}

        exitElement.SetExecute (app.Stop)

        navigationElements <- [usernameElement; passwordElement; exitElement]

        consoleView.SetElement (titleElement)
        consoleView.SetElement (usernameElement)
        consoleView.SetElement (passwordElement)
        consoleView.SetElement (exitElement)

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
