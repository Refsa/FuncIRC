#load "View/CLIView.fsx"

namespace FuncIRC_CLI

module Application = 
    open System
    open CLIView

    type InputState =
        {
            Line: string
            Key: ConsoleKey
        }

    let getTextInput (input: ConsoleKeyInfo): string =
        match input with
        | input when input.Key = ConsoleKey.LeftArrow -> ""
        | input when input.Key = ConsoleKey.RightArrow -> ""
        | input when input.Key = ConsoleKey.UpArrow -> ""
        | input when input.Key = ConsoleKey.DownArrow -> ""
        | _ -> string input.KeyChar

    /// Entry point for the CLI application
    type Application (cliView: CLIView) =
        let cliView = cliView

        let mutable running = false
        let mutable stateListener: InputState -> InputState = fun a -> {Line = ""; Key = ConsoleKey.NoName}

        /// TODO: Make this more functionally oriented by moving it out of the Application class
        let readLine (state: InputState): InputState =
            let readKey = Console.ReadKey()

            match readKey.Key with
            | ConsoleKey.Backspace ->
                let line = state.Line
                match line with
                | line when line.Length > 0 ->
                    line.Remove(line.Length - 1)
                | _ -> ""
            | _ -> 
                state.Line + (getTextInput readKey)
            |> fun state ->
                {Line = state; Key = readKey.Key}

        /// Starts the application loop, runs it with Async.RunSynchronously
        member this.Run () =
            running <- true

            let rec loop2 state =
                cliView.Draw()

                let feedbackState = readLine state |> stateListener

                if running then loop2 feedbackState
                else ()

            let loop =
                async {
                    loop2 {Line = ""; Key = ConsoleKey.NoName}
                }
            loop |> Async.RunSynchronously

            //let loop =
            //    async {
            //        while running do
            //            cliView.Draw()
            //            let feedbackState = readLine |> stateListener
            //            readLineInput <- feedbackState.Line
            //    }
            //loop |> Async.RunSynchronously

        /// Redraws last frame and stops the application loop if it's running
        member this.Stop () =
            match running with
            | true ->
                    cliView.Draw()
                    running <- false
            | false -> ()

        /// Binds the delegate to listen to changes to the application state
        member this.SetStateListener listener=
            stateListener <- listener