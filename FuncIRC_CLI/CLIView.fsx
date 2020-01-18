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
        let mutable color = color
        let mutable position = position
        let mutable executeDelegate: unit -> unit = ignore

        let canFocus = canFocus

        new () = CLIElement ("XXX", CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black), false)

        abstract member GetContent: string
        abstract member SetContent: string -> unit

        default this.GetContent = content
        default this.SetContent newContent = content <- newContent

        member this.Execute = executeDelegate
        member this.SetExecute func = executeDelegate <- func

        member this.GetColor = color
        member this.SetColor newColor = color <- newColor

        member this.CanFocus = canFocus

        member this.GetPosition = position.GetPosition()
        member this.SetPosition newPosition = position <- CLIPosition (newPosition, position.GetLine())

        member this.GetLine = position.GetLine()
        member this.SetLine newLine = position <- CLIPosition(position.GetPosition(), newLine)

        member this.PlaceElement target =
            placeOnString (target, content, this.GetPosition)

    type Button (content, position, color) =
        inherit CLIElement (content, position, color, true)

        override this.SetContent newContent = ()

    type Label (content, position, color) =
        inherit CLIElement (content, position, color, false)

    type TextField (content, position, color) =
        inherit CLIElement (content, position, color, true)

        let mutable placeholderText = "PLACEHOLDER"
        let mutable text = ""

        override this.GetContent = 
            match text with
            | text when text.Length = 0 -> content.Replace("$t", placeholderText)
            | _ -> content.Replace ("$t", text)

        override this.SetContent newContent =
            text <- newContent

        member this.PlaceholderText = placeholderText
        member this.Text = text

        member this.SetPlaceholderText pt = placeholderText <- pt
        member this.SetText t = text <- t

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
                                        let elementEndPosition = e.GetPosition + e.GetContent.Length
                                        if elementEndPosition > furtherstElementPosition then furtherstElementPosition <- elementEndPosition
                                        cprintf e.GetColor.ForegroundColor e.GetColor.BackgroundColor (toStringFormat (e.GetContent))
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