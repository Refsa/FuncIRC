#load "StringHelpers.fsx"

namespace FuncIRC

open StringHelpers

module GeneralHelpers =
    /// <summary>
    /// Factory function to extract a string list from a <string option> type
    /// </summary>
    let extractList (target: string option, method: string -> string list) =
        match target with
        | Some t -> Some (method t)
        | None -> None

    /// <summary>
    /// Factory function to remove elements from an array
    /// </summary>
    let arrayRemove (arr: 'a array) (method: 'a -> bool) = arr |> Array.where (method >> not)

    /// <summary>
    /// Removes all empty strings ("") from an array
    /// </summary>
    let stringArrayRemoveEmpty (stringArray: string array) =
        stringArray
        |> Array.where (stringIsEmpty)

    /// <summary>
    /// Tries to parse a string as an int
    /// </summary>
    /// <returns> Some of the int value if successful, None if not </returns>
    let tryParseInt (target: string) =
        try
            let parsed = int target
            Some parsed
        with
        | _ -> None

    /// <summary>
    /// Active pattern to parse a string as an int
    /// IntParsed returns the value parsed as an int
    /// InvalidParse returns nothing on an unsuccessful parse
    /// </summary>
    let (|IntParsed|InvalidParse|) (target: string) =
        let parsed = tryParseInt target

        match parsed with
        | Some parsed -> IntParsed parsed
        | None -> InvalidParse

    /// <summary> Adds item to end of listG. Inverse of the default @ operator </summary>
    let (@+) listG item =
        listG @ [item]

    /// <summary> Removes items from list that is euqal to item </summary>
    let (@!) listG item =
        listG |> List.where ((<>) item)

    /// <summary> Keeps items in list that is equal to item </summary>
    let (@=) listG item =
        listG |> List.where ((=) item)