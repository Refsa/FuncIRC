
#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "IRCTestInfo.fsx"
#load "ConsoleHelpers.fsx"
#load "GeneralHelpers.fsx"
#load "View/CLIView.fsx"
#load "Application.fsx"
#load "ApplicationState.fsx"

namespace FuncIRC_CLI

module CLI =
    open System

    open Application
    open ApplicationState
    open ConsoleHelpers
    open CLIView
    open CLIElement
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
    let titleElement = Label(titleString, CLIPosition(consoleSize.Width / 2 - titleString.Length / 2, 1), inverseColor)

    let usernameString = "Username: "
    let usernameElement = TextField(usernameString, CLIPosition(20, 4), defaultColor)

    let passwordString = "Password: "
    let passwordElement = TextField(passwordString, CLIPosition(20, 5), defaultColor)

    let logElement = TextField ("Log: ", CLIPosition (5, consoleSize.Height - 3), defaultColor)

    let loginString = "[Login]"
    let loginElement = Button(loginString, CLIPosition(20, 7), defaultColor)

    let exitString = "[Exit]"
    let exitElement = Button(exitString, CLIPosition(5, consoleSize.Height - 1), defaultColor)

    let defaultLine = (buildString " " consoleSize.Width).Remove(consoleSize.Width, 1)
                                                         .Remove(0, 1)
                                                         .Insert(0, "|")
                                                         .Insert(consoleSize.Width, "|")

    let topLine = (buildString "¨" consoleSize.Width)

    // Update
    let app = Application (consoleView)

    /// Prints out the current InputState content
    let printInputStateLine stateLine = 
        let state = placeOnString (defaultLine, stateLine, 20)
        consoleView.SetLine ({Content = state;
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, 10)

    /// Sets selection color for element and updates the Navigation model state
    let setFocusedNavigationElement elem index =
        if navigation.IsSome then
            navigation.Value.Focused.SetColor defaultColor

        navigation <- Some {Focused = elem; Index = index}
        navigation.Value.Focused.SetColor inverseColor

    /// Handles ArrowKey navigation input
    let navigate (state: InputState): InputState =
        if navigationElements.Length = 0 then state // No elements to navigate
        else // Handle navigation

        // Check if an element is currently focused
        match navigation with
        | None -> 
            setFocusedNavigationElement navigationElements.[0] 0
        | Some nav -> // Element is in focus, move to next element based on navigation input
            match state.Key with
            | ConsoleKey.DownArrow ->
                let navElemsEnd = navigationElements.Length - 1
                let index = nav.Index + 1
                index |> fun x -> if x > navElemsEnd then 0 else index
            | ConsoleKey.UpArrow -> 
                let index = nav.Index - 1
                index |> fun x -> if x < 0 then navigationElements.Length - 1 else index
            | _ -> -1
            |> fun i -> 
                if i >= 0 then
                    setFocusedNavigationElement navigationElements.[i] i

        {
            Line = navigation.Value.Focused.GetText
            Key = state.Key
        }

    /// Entry point for InputState handler from application
    let applicationStateHandler (state: ApplicationState): ApplicationState =
        // Handle the state and give feedback on changes
        match state.InputState.Key with
        | ConsoleKey.Enter when state.InputState.Line = "Quit" -> {Running = false; InputState = state.InputState}
        | IsNavigationInput ck -> 
            { Running = true; InputState = navigate state.InputState }
        | _ -> state
        |> fun stateFeedback ->
            match navigation with
            | Some nav -> nav.Focused.Execute stateFeedback
            | None -> stateFeedback


    do
        Console.Title <- "FuncIRC CLI"
        //Console.TreatControlCAsInput <- true

        app.SetStateListener applicationStateHandler

        let exitApp appState = 
            appState.InputState.Key
            |> function
            | ConsoleKey.Enter ->
                { Running = false; InputState = appState.InputState }
            | _ -> appState
            
        exitElement.SetExecuteDelegate exitApp

        navigationElements <- [usernameElement; passwordElement; loginElement; exitElement]

        consoleView.SetElement (titleElement)
        consoleView.SetElement (logElement)
        consoleView.SetElements (navigationElements)

        consoleView.SetLine ({Content = (buildString "=" consoleSize.Width);
                              ForegroundColor = ConsoleColor.Red;
                              BackgroundColor = ConsoleColor.Black}, 0)

        consoleView.SetLine ({Content = (buildString "=" consoleSize.Width);
                              ForegroundColor = ConsoleColor.Red;
                              BackgroundColor = ConsoleColor.Black}, 2)

        consoleView.SetLine ({Content = topLine;
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, consoleSize.Height)

        consoleView.SetBaseForegroundColor ConsoleColor.Green
        consoleView.SetBaseBackgroundColor ConsoleColor.Black

    [<EntryPoint>]
    let main argv =
        Console.Clear()
        (app.Run())

        //messageSplit testMessage0
        0 // return an integer exit code
