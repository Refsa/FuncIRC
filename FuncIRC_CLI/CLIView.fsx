#load "ConsoleHelpers.fsx"
#load "GeneralHelpers.fsx"

namespace FuncIRC_CLI

module CLIView =
    open System
    open ConsoleHelpers
    open GeneralHelpers

    /// Base class for creating elements to be placed and interacted with in the console window
    /// content: String content of the line
    /// position: x position of the element in the console window
    /// line: y position of the element in the console window
    /// foregroundColor: Foreground color of the element
    /// backgroundColor: Background color of the element
    type CLIElement(content, position, line, foregroundColor, backgroundColor) =
        let mutable content = content
        let mutable position = position
        let mutable line = line

        let foregroundColor = foregroundColor
        let backgroundColor = backgroundColor

        new () = CLIElement ("", 0, 0, ConsoleColor.Green, ConsoleColor.Black)

        member this.SetContent newContent = content <- newContent
        member this.SetPosition newPosition = position <- newPosition
        member this.SetLine newLine = line <- newLine

    /// Represents a line of text in the CLI
    type CLILine =
        {
            Content: Printf.StringFormat<unit, unit>
            ForegroundColor: ConsoleColor
            BackgroundColor: ConsoleColor
        }

    /// Represents the content of the CLI
    type CLIView (maxLines: int, maxWidth: int) =
        let maxLines = maxLines
        let maxWidth = maxWidth

        let mutable cliLines: CLILine array = [||]

        let mutable foregroundColor: ConsoleColor = ConsoleColor.Green
        let mutable backgroundColor: ConsoleColor = ConsoleColor.Black

        let defaultLine = (buildString " " maxWidth).Remove(maxWidth, 1)
                                                    .Remove(0, 1)
                                                    .Insert(0, "|")
                                                    .Insert(maxWidth, "|")

        do

            cliLines <- [|
                            for i in 0..maxLines -> 
                                                {Content = toStringFormat defaultLine; 
                                                 ForegroundColor = foregroundColor; 
                                                 BackgroundColor = backgroundColor}
                        |]

        /// Sets the content of <line> to <content>
        member this.SetLine (content: CLILine, line: int) =
            match line with
            | line when line <= maxLines -> cliLines.[line] <- content
            | _ -> ()

        /// Sets the base foreground color to <color>
        member this.SetBaseForegroundColor color =
            foregroundColor <- color

        /// Sets the base background color to <color>
        member this.SetBaseBackgroundColor color = 
            backgroundColor <- color

        /// Clears the current content of the console and redraws the content in cliLines
        member this.Draw () =
            Console.Clear()

            cliLines
            |> Array.iter (fun line -> (
                                        cprintfn line.ForegroundColor line.BackgroundColor line.Content
                                        )
                           )