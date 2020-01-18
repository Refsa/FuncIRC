#load "CLIView.fsx"

namespace FuncIRC_CLI

module Application = 
    open System
    open CLIView

    /// Entry point for the CLI application
    type Application (cliView: CLIView) =
        let cliView = cliView
        let mutable running = true

        let mutable state = ""
        let mutable readLineInput = ""

        let mutable stateListener: string -> unit = ignore

        let readLine() =
            let readKey = Console.ReadKey()

            match readKey.Key with
            | ConsoleKey.Enter -> 
                    state <- readLineInput
                    readLineInput <- ""
            | _ -> 
                    readLineInput <- readLineInput + (string readKey.KeyChar)
                    state <- readLineInput

        /// Starts the application loop
        /// TODO: Remove the handling of events from the local space
        member this.Run () =
            let loop =
                async {
                    while running do
                        cliView.Draw()
                        readLine()
                        stateListener state
                }

            loop |> Async.RunSynchronously

        member this.Stop () =
            match running with
            | true ->
                    cliView.Draw()
                    running <- false
            | false -> ()

        /// Binds the delegate to listen to changes to the application state
        member this.SetStateListener (listener: string -> unit) =
            stateListener <- listener