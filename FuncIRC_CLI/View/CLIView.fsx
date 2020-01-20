#load "../Utils/ConsoleHelpers.fsx"
#load "../Utils/GeneralHelpers.fsx"
#load "../Model/ApplicationState.fsx"
#load "CLIElement.fsx"

namespace FuncIRC_CLI

module CLIView =
    open System
    open CLIElement
    open ConsoleHelpers
    open GeneralHelpers
    open ApplicationState

    /// Represents a line of text in the CLI
    type CLILine =
        {
            Content: string
            ForegroundColor: ConsoleColor
            BackgroundColor: ConsoleColor
        }

    /// Represents the content of the CLI
    type CLIView (maxLines: int, maxWidth: int) =
        let maxLines = maxLines
        let maxWidth = maxWidth

        let mutable cliLines: CLILine array = [||]
        let mutable cliElements: CLIElement list = []

        let mutable foregroundColor: ConsoleColor = ConsoleColor.Green
        let mutable backgroundColor: ConsoleColor = ConsoleColor.Black

        let defaultLine = (buildString " " maxWidth).Remove(maxWidth - 1, 1)
                                                    .Remove(0, 1)
                                                    .Insert(0, "|")
                                                    .Insert(maxWidth - 1, "|")

        /// Sorts the content of cliElements based on position and line
        let sortElements =
            cliElements <- 
                        cliElements
                        |> List.sortBy (fun x -> ( x.GetPosition ))
                        |> List.sortBy (fun x -> ( x.GetLine ))

        // Constructor
        do
            cliLines <- [| for i in 0..maxLines -> {Content = defaultLine; 
                                                    ForegroundColor = foregroundColor; 
                                                    BackgroundColor = backgroundColor} |]

        /// Sets the content of <line> to <content>
        member this.SetLine (content: CLILine, line: int) =
            match line with
            | line when line <= maxLines -> cliLines.[line] <- content
            | _ -> ()

        /// Adds a CLIElement to be drawn with the view
        member this.AddElement (element: CLIElement) =
            cliElements <- element :: cliElements
            sortElements

        /// Adds a range of CLIElements to be drawn with the view
        member this.AddElements (elements: CLIElement list) =
            cliElements <- elements @ cliElements
            sortElements

        /// Sets the base foreground color to <color>
        member this.SetBaseForegroundColor color =
            foregroundColor <- color

        /// Sets the base background color to <color>
        member this.SetBaseBackgroundColor color = 
            backgroundColor <- color

        /// Executes all CLIElements in view with ApplicationState as an Empty State
        member this.ExecuteNoState() =
            let noState = {Running = false; InputState = {Line = ""; Key = ConsoleKey.NoName}}
            cliElements
            |> List.iter (fun e -> (e.Execute noState |> ignore))

        /// Clears the current content of the console and redraws the content in cliLines
        member this.Draw () =
            Console.SetCursorPosition (0, 0)
            cliLines 
            |> Array.iter 
                (
                    fun line ->
                        cprintfn line.ForegroundColor line.BackgroundColor (toStringFormat line.Content)
                )

            cliElements
            |> List.iter
                (
                    fun element ->
                        element.Draw
                )