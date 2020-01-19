namespace FuncIRC_CLI

module ConsoleHelpers =
    open System

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

    /// Defines the width and height of text in the CLI
    type ConsoleSize =
        {
            Width: int
            Height: int
        }

    /// Sets the foreground/background color of console to <fc>/<bc> and resets it back to the previous after it's Disposed
    let consoleColor (fc: ConsoleColor, bc: ConsoleColor) =
        let currentfc = Console.ForegroundColor
        let currentbc = Console.BackgroundColor

        Console.ForegroundColor <- fc
        Console.BackgroundColor <- bc

        {
            new IDisposable with
                member x.Dispose () = 
                                    Console.ForegroundColor <- currentfc
                                    Console.BackgroundColor <- currentbc
        }

    /// Prints the string <str> with printf to the console with the given <fc> and <bc>
    let cprintf fc bc str  = Printf.kprintf (fun s -> use c = consoleColor (fc, bc) in printf "%s" s) str
    /// Prints the string <str> with printfn to the console with the given <fc> and <bc>
    let cprintfn fc bc str = Printf.kprintf (fun s -> use c = consoleColor (fc, bc) in printfn "%s" s) str

    /// Checks if given ConsoleKey is one of the arrow keys
    let (|IsNavigationInput|_|) ck =
        match ck with
        | ConsoleKey.UpArrow 
        | ConsoleKey.DownArrow 
        | ConsoleKey.LeftArrow 
        | ConsoleKey.RightArrow -> Some ck
        | _ -> None