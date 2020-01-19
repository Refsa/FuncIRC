namespace FuncIRC_CLI

module GeneralHelpers =
    open System

    /// Exception to throw when mishandling strings
    exception StringPlacementException of string

    /// Appends <content> to a string <count> times
    let buildString content count : string =
        let rec buildStringRec (c: string, i: int) : string =
            match i with
            | i when i > 0 -> buildStringRec (c + content, i - 1)
            | _ -> c

        buildStringRec (content, count)

    /// Converts the input string <content> to Printf.StringFormat<unit, unit>
    let toStringFormat content : Printf.StringFormat<unit, unit> =
        Printf.StringFormat<unit, unit> (content)

    /// Centers the string <element> on the <target> string
    let centerOnString (target: string, element: string) : string =
        target.Remove(target.Length / 2 - element.Length / 2, element.Length)
              .Insert (target.Length / 2 - element.Length / 2, element)

    /// Places the <target> string on the <element> string starting at position <pos>
    let placeOnString (target: string, element: string, pos: int): string =
        match pos with
        | pos when pos + element.Length >= target.Length -> raise (StringPlacementException ("pos + element.Length was longer than target.Length"))
        | _ -> target.Remove(pos, element.Length).Insert(pos, element)