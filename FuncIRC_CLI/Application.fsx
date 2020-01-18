#load "CLIView.fsx"

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

        let mutable inputState = {Line = ""; Key = ConsoleKey.NoName}
        let mutable state = ""
        let mutable readLineInput = ""

        let mutable stateListener: InputState -> unit = ignore

        let readLine() =
            let readKey = Console.ReadKey()

            match readKey.Key with
            | ConsoleKey.Enter -> 
                    state <- readLineInput
                    readLineInput <- ""
            | ConsoleKey.Backspace ->
                    readLineInput <- 
                        match readLineInput with
                        | readLineInput when readLineInput.Length > 0 ->
                            readLineInput.Remove(readLineInput.Length - 1)
                        | _ -> ""
                    state <- readLineInput
            | _ -> 
                    readLineInput <- readLineInput + (getTextInput readKey)
                    state <- readLineInput

            inputState <- {Line = state; Key = readKey.Key}

        /// Starts the application loop
        /// TODO: Remove the handling of events from the local space
        member this.Run () =
            running <- true
            let loop =
                async {
                    while running do
                        cliView.Draw()
                        readLine()
                        stateListener inputState
                }

            loop |> Async.RunSynchronously

        member this.Stop () =
            match running with
            | true ->
                    cliView.Draw()
                    running <- false
            | false -> ()

        /// Binds the delegate to listen to changes to the application state
        member this.SetStateListener (listener: InputState -> unit) =
            stateListener <- listener