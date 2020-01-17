namespace FuncIRC_CLI

module GeneralHelpers =
    /// Appends content to a string count times
    let buildString content count : string =
        let rec buildStringRec (c: string, i: int) : string =
            match i with
            | i when i > 0 -> buildStringRec (c + content, i - 1)
            | _ -> c

        buildStringRec (content, count)

    let toStringFormat content : Printf.StringFormat<unit, unit> =
        Printf.StringFormat<unit, unit> (content)