namespace FuncIRC_CLI

module ConsoleHelpers =
    open System

    type ConsoleSize =
        {
            Width: int
            Height: int
        }

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

    let cprintf fc bc str  = Printf.kprintf (fun s -> use c = consoleColor (fc, bc) in printf "%s" s) str
    let cprintfn fc bc str = Printf.kprintf (fun s -> use c = consoleColor (fc, bc) in printfn "%s" s) str