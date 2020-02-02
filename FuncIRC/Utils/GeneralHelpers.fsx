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

    let stringIsEmpty (target: string) = target = ""

    let arrayRemove (arr: 'a array) (method: 'a -> bool) = arr |> Array.where (method >> not)

    let stringArrayRemoveEmpty (stringArray: string array) =
        stringArray
        |> Array.where 
            (fun x ->
                x <> ""
            )        

    let tryParseInt (target: string) =
        try
            let parsed = int target
            Some parsed
        with
        | _ -> None

    let (|IntParsed|InvalidParse|) (target: string) =
        let parsed = tryParseInt target

        match parsed with
        | Some parsed -> IntParsed int
        | None -> InvalidParse