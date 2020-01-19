#load "View/CLIView.fsx"
#load "ApplicationState.fsx"

namespace FuncIRC_CLI

module Application = 
    open System
    open ApplicationState
    open CLIView

    let getTextInput (input: ConsoleKeyInfo): string =
        match input with
        | input when input.Key = ConsoleKey.LeftArrow -> ""
        | input when input.Key = ConsoleKey.RightArrow -> ""
        | input when input.Key = ConsoleKey.UpArrow -> ""
        | input when input.Key = ConsoleKey.DownArrow -> ""
        | _ -> string input.KeyChar

    /// Container class for the core application loop
    type Application (cliView: CLIView) =
        let cliView = cliView

        let mutable stateListener: ApplicationState -> ApplicationState = 
            fun a -> {Running = false; InputState = {Line = ""; Key = ConsoleKey.NoName}}

        /// TODO: Make this more functionally oriented by moving it out of the Application class
        let readLine (state: InputState): InputState =
            let readKey = Console.ReadKey()

            match readKey.Key with
            | ConsoleKey.Enter -> state.Line
            | ConsoleKey.Backspace ->
                state.Line |> 
                function
                | line when line.Length > 0 ->
                    line.Remove(line.Length - 1)
                | _ -> ""
            | _ -> 
                state.Line + (getTextInput readKey)
            |> fun state ->
                {Line = state; Key = readKey.Key}

        /// Starts the application loop, runs it with Async.RunSynchronously
        member this.Run () =
            let rec loop appState =
                cliView.Draw()

                {
                    Running = true
                    InputState = readLine appState.InputState
                } 
                |> stateListener
                |> fun feedbackState ->
                    if feedbackState.Running then loop feedbackState
                    else this.Stop()

            let startLoop =
                async {
                    loop {Running = true; InputState = {Line = ""; Key = ConsoleKey.NoName}}
                }

            startLoop |> Async.RunSynchronously

        /// Redraws last frame and stops the application loop if it's running
        member this.Stop () =
            cliView.Draw()

        /// Binds the delegate to listen to changes to the application state
        member this.SetStateListener listener =
            stateListener <- listener