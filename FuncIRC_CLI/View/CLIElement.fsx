#load "../Utils/ConsoleHelpers.fsx"
#load "../Utils/GeneralHelpers.fsx"
#load "../Model/ApplicationState.fsx"

namespace FuncIRC_CLI

module CLIElement =
    open System
    open ApplicationState
    open ConsoleHelpers
    open GeneralHelpers
    
    // TODO:
    // There are more functional ways to do this but it might overcomplicate the problem at hand

    /// Base class for creating elements to be placed and interacted with in the console window
    /// content: String content of the line
    /// position: x and y position of the element
    /// color: CLIColor record containing foreground and background color
    type CLIElement(content, position, color, canFocus) =
        let mutable content = content
        let mutable color = color
        let mutable position = position

        let canFocus = canFocus

        new () = CLIElement ("XXX", CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black), false)

        abstract member GetContent: string
        abstract member SetContent: string -> unit
        abstract member GetText: string
        abstract member GetWidth: int
        abstract member Draw: unit
        abstract member Execute: ApplicationState -> ApplicationState

        default this.GetContent = content
        default this.SetContent newContent = content <- newContent
        default this.GetText = ""
        default this.GetWidth = this.GetContent.Length

        default this.Draw =
            cprintf color.ForegroundColor color.BackgroundColor (toStringFormat this.GetContent)
            
        default this.Execute appState =
            this.SetContent appState.InputState.Line
            appState

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
        override this.Execute appState = executeDelegate appState
        member this.SetExecuteDelegate func = executeDelegate <- func

    /// Simply a direct wrapper around CLIElement. Draws content on elements position
    type Label (content, position, color) =
        inherit CLIElement (content, position, color, false)

    /// TextField has a label stored in content and input text stored in text
    type TextField (content, position, color) =
        inherit CLIElement (content, position, color, true)

        let placeholderText = "<PLACEHOLDER>"
        let mutable text = ""

        override this.Draw =
            cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat ("[ " + content))
            match text with
            | "" ->
                cprintf ConsoleColor.DarkGray this.GetColor.BackgroundColor (toStringFormat placeholderText)
            | _ ->
                cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat text)
            cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat " ]")

        override this.GetText = text

        override this.GetContent = 
            match text with
            | text when text.Length = 0 -> "[ " + content + placeholderText + " ]"
            | _ -> "[ " + content + text + " ]"

        override this.SetContent newContent = newContent |> this.SetText
        member this.PlaceholderText = placeholderText
        member this.Text = text
        member this.SetText t = text <- t

    type PasswordField (content, position, color) =
        inherit TextField (content, position, color)

    type ProgressBar (content, position, color) =
        inherit CLIElement (content, position, color, false)
        
        let mutable progress = 0
        let width = 64

        override this.GetWidth = width + 4

        override this.Draw =
            let progressString = this.GetContent

            cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat ("[ "))
            cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat (progressString))

            let restString = buildString "_" (width - progress)
            cprintf this.GetColor.ForegroundColor this.GetColor.BackgroundColor (toStringFormat (restString + " ]"))

        override this.GetContent =
            buildString content progress

        override this.Execute appState =
            progress <- progress + 1

            appState