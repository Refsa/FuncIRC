#load "../Model/ApplicationState.fsx"

namespace FuncIRC_CLI

module Application = 
    open System
    open ApplicationState

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
    type Application (viewListener_: unit -> unit, stateListener_: ApplicationState -> ApplicationState) =
        let stateListener = stateListener_
        let viewListener = viewListener_

        let initialState = {Running = true; InputState = {Line = ""; Key = ConsoleKey.NoName}}

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
            let loopCanceller = new Threading.CancellationTokenSource()

            // This is the application loop
            let rec loop appState =
                async {
                    viewListener ()

                    let feedbackState =
                        {
                            Running = true
                            InputState = readLine appState.InputState
                        } 
                        |> stateListener

                    if feedbackState.Running then return! loop feedbackState
                    else loopCanceller.Cancel()
                }
            
            let worker() = loop initialState

            try
                Async.StartImmediateAsTask(worker(), loopCanceller.Token) |> ignore
            with
                :? OperationCanceledException -> ()

        /// Redraws last frame and stops the application loop if it's running
        member this.Stop () =
            ()