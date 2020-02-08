namespace FuncIRC
open System.Text

module StringHelpers =
    open System

    /// Latin-1 Encoding object
    let latin1Encoding = Encoding.GetEncoding("ISO-8859-1") 
    /// UTF-8 Encoding object
    let utf8Encoding = Encoding.UTF8 

    /// Raised when the incoming byte array/stream couldn't be parsed with either UTF-8 or Latin-1
    exception IncomingByteMessageParsingException

    /// Takes a byte array and first attemps to decode using UTF8, uses Latin-1 if that fails
    /// raises: <typeref "IncomingByteMessageParsingException">
    let parseByteString (data: byte array) =
        try
            utf8Encoding.GetString(data, 0, data.Length)
        with
            e ->
                try 
                    latin1Encoding.GetString(data, 0, data.Length)
                with
                | e -> raise IncomingByteMessageParsingException

    /// Trims the first character of a string if it's the given character
    /// <params name="target"> string to trim </params>
    /// <params name="character"> character to look for and trim off the start </params>
    /// <returns> A trimmed string or just the original string </returns>
    let stringTrimFirstIf (target:string, character: char): string =
        if target.[0] = character then
            target.[1..target.Length - 1]
        else
            target

    /// Trims the last character of a string if it's the given character
    /// <params name="target"> string to trim </params>
    /// <params name="character"> character to look for and trim off the end </params>
    /// <returns> A trimmed string or just the original string </returns>
    let stringTrimLastIf (target:string, character: char): string =
        if target.[target.Length - 1] = character then
            target.[0..target.Length - 2]
        else
            target

    /// Takes the value from a string option if it is some
    /// <returns> the string value if it is Some, empty string if None </returns>
    let stringFromStringOption (target: string option) =
        match target with
        | Some target -> target
        | None -> ""

    /// Checks if the given string is only alphanumeric
    /// <params name="string"> string to look through </params>
    /// <returns> true if its an alphanumeric string </returns>
    let stringIsOnlyAlphaNumeric (target: string) =
        target 
        |> Seq.toList
        |> List.forall
            (fun c ->
                Char.IsLetterOrDigit c
            ) 

    /// Checks that the given string is alphanumeric and doesn't contain any of the given characters
    /// <params name="target"> string to look through </params>
    /// <params name="except"> Array of chars to check for </params>
    /// <returns> true if it is alphanumeric and contains none of the chars in the except array </returns>
    let stringIsOnlyAlphaNumericExcept (target: string) (except: char array) =
        target 
        |> Seq.toList
        |> List.forall
            (fun c ->
                if not (Array.contains c except) then
                    Char.IsLetterOrDigit c
                else
                    true
            )

    /// Checks that a string does not contain any of the given chars in the characters array
    /// <params name="target"> target string to look through </params>
    /// <params name="characters"> array of characters to look for </params>
    /// <returns> true if it contains any of the given characters </returns>
    let stringDoesNotContain (target: string) (characters: char array) =
        target
        |> Seq.toList
        |> List.forall
            (fun c ->
                match c with
                | c when Array.contains c characters -> false
                | _ -> true
            )