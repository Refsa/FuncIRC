namespace FuncIRC_CLI

module ApplicationState =
    open System

    type InputState =
        {
            Line: string
            Key: ConsoleKey
        }

    type ApplicationState =
        {
            Running: bool
            InputState: InputState
        }