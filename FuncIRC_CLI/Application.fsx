#load "CLIView.fsx"

namespace FuncIRC_CLI

module Application = 
    open System
    open CLIView

    /// Entry point for the CLI application
    type Application (cliView: CLIView) =
        let cliView = cliView

        let mutable state = ""

        let mutable stateListener: string -> unit = ignore

        /// Starts the application loop
        /// TODO: Remove the handling of events from the local space
        member this.Run () =
            let loop =
                async {
                    let mutable running = true
                    while running do
                        cliView.Draw()
                        state <- Console.ReadLine ()
                        stateListener state

                        match state with
                        | "quit" -> running <- false
                        | _ -> ()
                }

            loop |> Async.RunSynchronously

        /// Binds the delegate to listen to changes to the application state
        member this.SetStateListener (listener: string -> unit) =
            stateListener <- listener