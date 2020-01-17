#load "CLIView.fsx"

namespace FuncIRC_CLI

module Application = 
    open System
    open CLIView

    type Application (cliView: CLIView) =
        let cliView = cliView

        let mutable state = ""

        let mutable stateListener: string -> unit = ignore

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

        member this.SetStateListener (listener: string -> unit) =
            stateListener <- listener