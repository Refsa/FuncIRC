#load "../Model/ApplicationState.fsx"

namespace FuncIRC_CLI

module ButtonFunctions =
    open System
    open ApplicationState

    let exitApp appState = 
        appState.InputState.Key
        |> function
        | ConsoleKey.Enter ->
            Console.Clear()
            { Running = false; InputState = appState.InputState }
        | _ -> appState