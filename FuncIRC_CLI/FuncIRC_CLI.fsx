
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
    open System.IO

    open System.Text
    open System.Threading
    open System.Threading.Tasks

    open FuncIRC.ConnectionClient
    open FuncIRC.IRCStreamReader
    open FuncIRC.IRCClient

    open Application
    open ApplicationState
    open ConsoleHelpers
    open NavigationState
    open LoginView
    open StartupView

    let consoleSize = {Width = 128; Height = 32}

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

    /// Entry point for view handler from application
    let applicationViewHandler() =
        currentView.CLIView.Draw()

    let app = Application (applicationViewHandler, applicationStateHandler)

    /// Test task to run in the background and update the progress bar on startupView
    let testAsyncTask() =
        let canceller = new Threading.CancellationTokenSource()
        let rec worker progress =
            async {
                Threading.Thread.Sleep (15)
                match currentView with
                | currentView when currentView = startupView -> currentView.CLIView.ExecuteNoState()
                | _ -> ()

                if progress = 99 then 
                    currentView <- loginView
                    applicationViewHandler()
                    canceller.Cancel()
                else return! worker (progress + 1)
            }

        let runner() = worker 0
        try
            Async.StartAsTask(runner(), Threading.Tasks.TaskCreationOptions(), canceller.Token) |> ignore
        with
            :? OperationCanceledException -> ()

    let ircLoop (client: Client, token: CancellationTokenSource) =
        Async.StartAsTask((readStream client receivedDataHandler), TaskCreationOptions(), token.Token) |> ignore

        client |> sendRegistrationMessage <| ("testnick", "testuser", "some name", "")

        printfn "### CLI LOOP ###"
        while Console.ReadKey().Key <> ConsoleKey.Q do
            ()
        token

    [<EntryPoint>]
    let main argv =
        Console.Title <- "FuncIRC CLI"

        //consoleSize
        //|> fun cs ->
        //    Console.SetWindowSize (cs.Width, cs.Height)
        //    Console.SetBufferSize (cs.Width, cs.Height)

        //Console.Clear()
        //Console.SetCursorPosition (0, 0)

        //testAsyncTask()
        //(app.Run())

        ircClient "127.0.0.1" 6667 
        |> ircClientHandler
        |> ircLoop
        |> closeIrcClient

        0 // return an integer exit code
