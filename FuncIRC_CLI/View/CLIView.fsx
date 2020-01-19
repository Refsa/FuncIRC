#load "../Utils/ConsoleHelpers.fsx"
#load "../Utils/GeneralHelpers.fsx"
#load "CLIElement.fsx"

namespace FuncIRC_CLI

module CLIView =
    open System
    open CLIElement
    open ConsoleHelpers
    open GeneralHelpers

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
        member this.SetElement (element: CLIElement) =
            cliElements <- element :: cliElements
            sortElements

        /// Adds a range of CLIElements to be drawn with the view
        member this.SetElements (elements: CLIElement list) =
            cliElements <- elements @ cliElements
            sortElements

        /// Sets the base foreground color to <color>
        member this.SetBaseForegroundColor color =
            foregroundColor <- color

        /// Sets the base background color to <color>
        member this.SetBaseBackgroundColor color = 
            backgroundColor <- color

        /// Draws the <elements> content inline into the <line> content
        member this.DrawLine (line: CLILine, elements: CLIElement list) =
            match elements with
            | elements when elements.Length > 0 ->
                let firstElementPosition = elements.[0].GetPosition
                let mutable furtherstElementPosition = 0

                cprintf line.ForegroundColor line.BackgroundColor (toStringFormat (line.Content.[0..firstElementPosition - 1]))

                elements
                |> List.iter (fun e ->(
                                        let elementEndPosition = e.GetPosition + e.GetWidth
                                        if elementEndPosition > furtherstElementPosition then furtherstElementPosition <- elementEndPosition
                                        e.Draw
                              ))

                cprintf line.ForegroundColor line.BackgroundColor (toStringFormat (line.Content.[furtherstElementPosition..line.Content.Length - 1]))
                cprintf line.ForegroundColor line.BackgroundColor (toStringFormat "\n")
            | _ -> 
                cprintfn line.ForegroundColor line.BackgroundColor (toStringFormat line.Content)

        /// Clears the current content of the console and redraws the content in cliLines
        member this.Draw () =
            Console.Clear()

            cliLines
            |> Array.iteri 
                    (fun index -> 
                        ( fun line ->
                            cliElements 
                            |> List.where (fun x -> x.GetLine = index)
                            |> List.sortBy (fun x -> x.GetPosition)
                            |> fun e -> this.DrawLine (line, e)
                        )
                    )