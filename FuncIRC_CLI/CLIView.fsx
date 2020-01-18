#load "ConsoleHelpers.fsx"
#load "GeneralHelpers.fsx"

namespace FuncIRC_CLI

module CLIView =
    open System
    open ConsoleHelpers
    open GeneralHelpers

    type CLIPosition (x: int, y: int) =
        let x = x
        let y = y

        member this.GetPosition () = x
        member this.GetLine () = y

    type CLIColor (fc: ConsoleColor, bc: ConsoleColor) =
        let foregroundColor = fc
        let backgroundColor = bc

        member this.ForegroundColor = foregroundColor
        member this.BackgroundColor = backgroundColor

    /// Base class for creating elements to be placed and interacted with in the console window
    /// content: String content of the line
    /// position: x position of the element in the console window
    /// line: y position of the element in the console window
    /// foregroundColor: Foreground color of the element
    /// backgroundColor: Background color of the element
    type CLIElement(content, position, color, canFocus) =
        let mutable content = content
        let color = color
        let mutable position = position
        let canFocus = canFocus

        new () = CLIElement ("XXX", CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black), false)

        member this.GetContent = content
        member this.SetContent newContent = content <- newContent

        member this.GetColor = color
        member this.CanFocus = canFocus

        member this.GetPosition = position.GetPosition()
        member this.SetPosition newPosition = position <- CLIPosition (newPosition, position.GetLine())

        member this.GetLine = position.GetLine()
        member this.SetLine newLine = position <- CLIPosition(position.GetPosition(), newLine)

        member this.PlaceElement target =
            placeOnString (target, content, this.GetPosition)

    type Label (content, position, color) =
        inherit CLIElement (content, position, color, false)

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

        let defaultLine = (buildString " " maxWidth).Remove(maxWidth, 1)
                                                    .Remove(0, 1)
                                                    .Insert(0, "|")
                                                    .Insert(maxWidth, "|")

        let sortElements () =
            cliElements <- 
                        cliElements
                        |> List.sortBy (fun x -> ( x.GetPosition ))
                        |> List.sortBy (fun x -> ( x.GetLine ))

        do
            cliLines <- [| for i in 0..maxLines -> {Content = defaultLine; 
                                                    ForegroundColor = foregroundColor; 
                                                    BackgroundColor = backgroundColor} |]

        /// Sets the content of <line> to <content>
        member this.SetLine (content: CLILine, line: int) =
            match line with
            | line when line <= maxLines -> cliLines.[line] <- content
            | _ -> ()

        member this.SetElement (element: CLIElement) =
            cliElements <- element :: cliElements
            sortElements()

        member this.SetElements (elements: CLIElement list) =
            cliElements <- elements @ cliElements
            sortElements()

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
            |> Array.iteri 
                    (fun index -> 
                        ( fun line ->
                            let mutable lineContent = line.Content
                            
                            cliElements 
                            |> List.where (fun x -> x.GetLine = index)
                            |> List.iter (fun e -> lineContent <- e.PlaceElement lineContent)

                            cprintfn line.ForegroundColor line.BackgroundColor (toStringFormat lineContent)
                        )
                    )