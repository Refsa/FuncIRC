namespace FuncIRC
open System.Text

module StringHelpers =
    open System

    let latin1Encoding = Encoding.GetEncoding("ISO-8859-1") // Latin-1
    let utf8Encoding = Encoding.UTF8 // UTF-8

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

    let stringTrimFirstIf (target:string, character: char): string =
        if target.[0] = character then
            target.[1..target.Length - 1]
        else
            target

    let stringTrimLastIf (target:string, character: char): string =
        if target.[target.Length - 1] = character then
            target.[0..target.Length - 2]
        else
            target

    let stringFromStringOption (target: string option) =
        match target with
        | Some target -> target
        | None -> ""

    let stringIsOnlyAlphaNumeric (target: string) =
        target 
        |> Seq.toList
        |> List.forall
            (fun c ->
                Char.IsLetterOrDigit c
            ) 

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