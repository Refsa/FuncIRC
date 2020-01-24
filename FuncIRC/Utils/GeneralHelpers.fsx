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

    let arrayRemove arr method = arr |> Array.where (method >> not)

    let stringArrayRemoveEmpty (stringArray: string array) =
        stringArray
        |> Array.where 
            (fun x ->
                x <> ""
            )