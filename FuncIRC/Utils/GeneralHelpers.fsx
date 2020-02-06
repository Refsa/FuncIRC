namespace FuncIRC

module GeneralHelpers =
    /// Factory function to extract a string list from a <string option> type
    let extractList (target: string option, method: string -> string list) =
        match target with
        | Some t -> Some (method t)
        | None -> None

    /// Factory function to extract string from a <string option> type
    let extractString (target: string option, method: string -> string) =
        match target with
        | Some t -> Some (method t)
        | None -> None

    /// Checks if a string is empty ("")
    /// <returns> true if string was empty </returns>
    let stringIsEmpty (target: string) = target = ""

    /// Factory function to remove elements from an array
    let arrayRemove (arr: 'a array) (method: 'a -> bool) = arr |> Array.where (method >> not)

    /// Removes all empty strings ("") from an array
    let stringArrayRemoveEmpty (stringArray: string array) =
        stringArray
        |> Array.where (stringIsEmpty)

    /// Tries to parse a string as an int
    /// <returns> Some of the int value if successful, None if not </returns>
    let tryParseInt (target: string) =
        try
            let parsed = int target
            Some parsed
        with
        | _ -> None

    /// Active pattern to parse a string as an int
    /// IntParsed returns the value parsed as an int
    /// InvalidParse returns nothing on an unsuccessful parse
    let (|IntParsed|InvalidParse|) (target: string) =
        let parsed = tryParseInt target

        match parsed with
        | Some parsed -> IntParsed parsed
        | None -> InvalidParse