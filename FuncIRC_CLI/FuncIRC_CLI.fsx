
#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "IRCTestInfo.fsx"
#load "Utils/ConsoleHelpers.fsx"
#load "Utils/GeneralHelpers.fsx"
#load "View/CLIView.fsx"
#load "Update/Application.fsx"
#load "Model/ApplicationState.fsx"
#load "Model/NavigationState.fsx"
#load "Update/Navigation.fsx"
#load "Update/ButtonFunctions.fsx"
#load "View/LoginView.fsx"
#load "View/StartupView.fsx"

namespace FuncIRC_CLI

module CLI =
    open System

    open Application
    open ApplicationState
    open ButtonFunctions
    open ConsoleHelpers
    open CLIView
    open CLIElement
    open FuncIRC.MessageParser
    open GeneralHelpers
    open IRCTestInfo
    open NavigationState
    open Navigation
    open LoginView
    open StartupView

    let consoleSize = {Width = 128; Height = 16}

    let loginView = setupLoginView(consoleSize)
    let startupView = setupStartupView(consoleSize)

    let views = [startupView; loginView]

    let mutable currentView = startupView

    /// Entry point for InputState handler from application
    let applicationStateHandler (state: ApplicationState): ApplicationState =
        // Handle the state and give feedback on changes
        match state.InputState.Key with
        | ConsoleKey.Enter when state.InputState.Line = "Quit" -> {Running = false; InputState = state.InputState}
        | IsNavigationInput ck -> { Running = true; InputState = currentView.Navigation.Navigate state.InputState }
        | _ -> state
        |> fun stateFeedback ->
            match currentView.Navigation.Focused with
            | Some nav -> nav.Focused.Execute stateFeedback
            | None -> stateFeedback

    let applicationViewHandler() =
        currentView.CLIView.Draw()

    let app = Application (applicationViewHandler, applicationStateHandler)

    do
        Console.Title <- "FuncIRC CLI"
        //Console.TreatControlCAsInput <- true

    [<EntryPoint>]
    let main argv =
        Console.Clear()
        (app.Run())
        0 // return an integer exit code
