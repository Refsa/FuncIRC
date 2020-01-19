#load "../Model/NavigationState.fsx"
#load "../View/CLIElement.fsx"
#load "../Utils/ConsoleHelpers.fsx"
#load "../Model/ApplicationState.fsx"

namespace FuncIRC_CLI

module Navigation =
    open System
    open NavigationState
    open CLIElement
    open ConsoleHelpers
    open ApplicationState

    type Navigation (defaultColor: CLIColor, focusedColor: CLIColor) =
        let mutable focused: NavigationState option = None
        let mutable elements: CLIElement list = []

        /// Sets selection color for element and updates the Navigation model state
        let setFocusedNavigationElement elem index =
            if focused.IsSome then
                focused.Value.Focused.SetColor defaultColor

            focused <- Some {Focused = elem; Index = index}
            focused.Value.Focused.SetColor focusedColor

        /// Handles ArrowKey navigation input
        member this.Navigate (state: InputState): InputState =
            if elements.Length = 0 then state // No elements to navigate
            else // Handle navigation

            // Check if an element is currently focused
            match focused with
            | None -> 
                setFocusedNavigationElement elements.[0] 0
            | Some nav -> // Element is in focus, move to next element based on navigation input
                match state.Key with
                | ConsoleKey.DownArrow ->
                    let navElemsEnd = elements.Length - 1
                    let index = nav.Index + 1
                    index |> fun x -> if x > navElemsEnd then 0 else index
                | ConsoleKey.UpArrow -> 
                    let index = nav.Index - 1
                    index |> fun x -> if x < 0 then elements.Length - 1 else index
                | _ -> -1
                |> fun i -> 
                    if i >= 0 then
                        setFocusedNavigationElement elements.[i] i

            {
                Line = focused.Value.Focused.GetText
                Key = state.Key
            }

        member this.SetElements elems = elements <- elems
        member this.Focused = focused