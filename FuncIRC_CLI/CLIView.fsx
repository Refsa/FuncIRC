#load "ConsoleHelpers.fsx"
#load "GeneralHelpers.fsx"

namespace FuncIRC_CLI

module CLIView =
    open System
    open ConsoleHelpers
    open GeneralHelpers

    type CLILine =
        {
            Content: Printf.StringFormat<unit, unit>
            ForegroundColor: ConsoleColor
            BackgroundColor: ConsoleColor
        }

    type CLIView (maxLines: int, maxWidth: int) as this =
        let maxLines = maxLines
        let maxWidth = maxWidth

        let mutable cliLines: CLILine array = [||]

        let mutable foregroundColor: ConsoleColor = ConsoleColor.Black
        let mutable backgroundColor: ConsoleColor = ConsoleColor.Cyan

        do
            cliLines <- [|
                            for i in 0..maxLines -> 
                                                {Content = toStringFormat (buildString "#" maxWidth); 
                                                 ForegroundColor = foregroundColor; 
                                                 BackgroundColor = backgroundColor}
                        |]

        member this.SetLine (content: CLILine, line: int) =
            match line with
            | line when line < maxLines -> cliLines.[line] <- content
            | _ -> ()

        member this.SetBaseForegroundColor color =
            foregroundColor <- color

        member this.SetBaseBackgroundColor color = 
            backgroundColor <- color

        member this.Draw () =
            Console.Clear()

            cliLines
            |> Array.iter (fun line -> (
                                        cprintfn line.ForegroundColor line.BackgroundColor line.Content
                                        )
                           )