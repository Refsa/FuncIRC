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

        let mutable readLineInput = ""

        let mutable stateListener: InputState -> InputState = fun a -> {Line = ""; Key = ConsoleKey.NoName}

        /// TODO: Make this more functionally oriented by moving it out of the Application class
        let readLine() =
            let readKey = Console.ReadKey()

            match readKey.Key with
            //| ConsoleKey.Enter ->
            //        let state = readLineInput
            //        readLineInput <- ""
            //        state
            | ConsoleKey.Backspace ->
                    readLineInput <- 
                        match readLineInput with
                        | readLineInput when readLineInput.Length > 0 ->
                            readLineInput.Remove(readLineInput.Length - 1)
                        | _ -> ""
                    readLineInput
            | _ -> 
                    readLineInput <- readLineInput + (getTextInput readKey)
                    readLineInput
            |> fun state ->
                {Line = state; Key = readKey.Key}

        /// Starts the application loop
        member this.Run () =
            running <- true
            let loop =
                async {
                    while running do
                        cliView.Draw()
                        
                        let feedbackState = readLine() |> stateListener
                        readLineInput <- feedbackState.Line
                }

            loop |> Async.RunSynchronously

        member this.Stop () =
            match running with
            | true ->
                    cliView.Draw()
                    running <- false
            | false -> ()

        /// Binds the delegate to listen to changes to the application state
        member this.SetStateListener listener=
            stateListener <- listener