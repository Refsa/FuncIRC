#load "../Utils/ConsoleHelpers.fsx"
#load "../Utils/GeneralHelpers.fsx"
#load "../Model/ApplicationState.fsx"

namespace FuncIRC_CLI

/// TODO:
/// Currently not a functional framework for views, there are ways to refactor this into using Records to store the data
/// and create functions that work on that data instead.
module CLIElement =
    open System
    open ApplicationState
    open ConsoleHelpers
    open GeneralHelpers
    
    /// Base class for creating elements to be placed and interacted with in the console window
    /// content: String content of the line
    /// position: x and y position of the element
    /// color: CLIColor record containing foreground and background color
    type CLIElement(content, position, color, canFocus) =
        let mutable content = content
        let mutable color = color
        let mutable position = position
        let mutable redrawDelegate: unit -> unit = ignore

        let canFocus = canFocus

        new () = CLIElement ("XXX", CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black), false)

        /// Should return the content to be displayed in the terminal as a string
        abstract member GetContent: string
        /// Should set the content of the element that can be changed
        abstract member SetContent: string -> unit
        /// Should retreive only the part of the elements content that can be changed
        abstract member GetText: string
        /// Should return the width of the content that is to be displayed
        abstract member GetWidth: int
        /// Handles drawing the element, buffer should be ready for this element to be drawn in place
        abstract member Draw: unit
        /// Called on the element to update and signal its internal state
        abstract member Execute: ApplicationState -> ApplicationState

        default this.GetContent = content
        default this.SetContent newContent = content <- newContent
        default this.GetText = ""
        default this.GetWidth = this.GetContent.Length

        default this.Draw =
            Console.SetCursorPosition (position.GetPosition(), position.GetLine())
            cprintf color.ForegroundColor color.BackgroundColor (toStringFormat this.GetContent)
            
        default this.Execute appState =
            if not appState.Running then appState
            else this.SetContent appState.InputState.Line; appState

        member this.GetColor = color
        member this.SetColor newColor = color <- newColor

        member this.CanFocus = canFocus

        member this.GetPosition = position.GetPosition()
        member this.SetPosition newPosition = position <- CLIPosition (newPosition, position.GetLine())

        member this.GetLine = position.GetLine()
        member this.SetLine newLine = position <- CLIPosition(position.GetPosition(), newLine)

        member this.PlaceElement target =
            placeOnString (target, content, this.GetPosition)

    /// Can be assigned a custom delegate to run
    type Button (content, position, color) =
        inherit CLIElement (content, position, color, true)

        let mutable executeDelegate: ApplicationState -> ApplicationState = id

        override this.SetContent newContent = ()
        override this.Execute appState = 
            if not appState.Running then appState
            else executeDelegate appState

        member this.SetExecuteDelegate func = executeDelegate <- func

    /// Simply a direct wrapper around CLIElement. Draws content on elements position
    type Label (content, position, color) =
        inherit CLIElement (content, position, color, false)

    /// TextField has a label stored in content and input text stored in text
    type TextField (content, position, color) =
        inherit CLIElement (content, position, color, true)

        let placeholderText = "<PLACEHOLDER>"
        let mutable text = ""

        abstract member Text: string

        override this.Draw =
            Console.SetCursorPosition (position.GetPosition(), position.GetLine())
            cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat ("[ " + content))
            match text with
            | "" ->
                cprintf ConsoleColor.DarkGray this.GetColor.BackgroundColor (toStringFormat placeholderText)
            | _ ->
                cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat this.Text)
            cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat " ]")

        override this.GetText = text
        member this.SetText t = text <- t

        override this.GetContent = 
            match text with
            | text when text.Length = 0 -> "[ " + content + placeholderText + " ]"
            | _ -> "[ " + content + text + " ]"

        override this.SetContent newContent = newContent |> this.SetText
        member this.PlaceholderText = placeholderText
        default this.Text = text

    /// TextField that hides the input
    type PasswordField (content, position, color) =
        inherit TextField (content, position, color)

        override this.Text =
            buildString "*" this.GetText.Length

    /// Progress bar
    type ProgressBar (content, position, color, _redrawDelegate) =
        inherit CLIElement (content, position, color, false)
        
        let redrawDelegate = _redrawDelegate

        let mutable progress = 0
        let width = 64

        let currentWidth() =
            float(progress) / 100.0
            |> fun p ->
                int(float(width) * p)

        override this.GetWidth = width + 4

        override this.Draw =
            Console.SetCursorPosition (position.GetPosition(), position.GetLine())
            let progressString = this.GetContent

            cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat ("[ "))
            cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat (progressString))

            let restString = buildString "_" (width - currentWidth())
            cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat (restString + " ]"))

        override this.GetContent =
            buildString content (currentWidth())

        override this.Execute appState =
            progress <- progress + 1
            if progress > 100 then progress <- 0

            redrawDelegate()
            appState