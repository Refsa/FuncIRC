#load "../ConsoleHelpers.fsx"
#load "../GeneralHelpers.fsx"

namespace FuncIRC_CLI

module CLIElement =
    open System
    open ConsoleHelpers
    open GeneralHelpers
    
    // TODO:
    // There are more functional ways to do this but it might overcomplicate the problem at hand

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
        abstract member GetText: string

        default this.GetContent = content
        default this.SetContent newContent = content <- newContent
        default this.GetText = ""

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

        let placeholderText = "PLACEHOLDER"
        let mutable text = ""

        override this.GetText = text

        override this.GetContent = 
            match text with
            | text when text.Length = 0 -> content.Replace("$t", placeholderText)
            | _ -> content.Replace ("$t", text)

        override this.SetContent newContent =
            text <- newContent

        member this.PlaceholderText = placeholderText
        member this.Text = text

        member this.SetText t = text <- t
