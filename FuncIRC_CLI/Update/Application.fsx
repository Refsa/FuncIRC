#load "../View/CLIView.fsx"
#load "../Model/ApplicationState.fsx"

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
    /// TODO: Currently stops from a common state, might be safer to use async cancellation
    ///       It shouldn't really matter since no other thread should depend on this one (in relation to a CLI App)
    ///       but to be usable in more contexts a refactor should be made
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
            // This is the application loop
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

            // This is an async wrapper around the main loop
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