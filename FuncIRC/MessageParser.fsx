namespace FuncIRC
#load "Utils.fsx"
#load "RegexHelpers.fsx"
#load "StringHelpers.fsx"

module MessageParser =

    open Utils
    open RegexHelpers
    open StringHelpers

    type Tag =
        { Key: string
          Value: string option }

    type Source =
        { Nick: string
          User: string
          Host: string }

    type Command =
        { Verb: string
          Params: string list option}

    type Message =
        { Tags: Tag list option
          Source: string option
          Verb: string option
          Params: string list option }

    let tagsRegex = @"^(@\S+)"
    let sourceRegex = @"^(:[\S.]+)"
    let commandRegex = @"^([a-zA-Z0-9]+.+)"

    let extractTags (tagsString: string) =
        ((tagsString.TrimStart('@').TrimStart(' ').Split(';')) |> Array.toList)

    let extractSource (sourceString: string) =
        (sourceString.TrimStart(':').TrimStart(' '))

    let extractCommand (commandString: string) =
        (commandString.TrimStart(' '))

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

    /// Takes the list of tags extracted in messageSplit and creates a list of Tag Records from them
    let parseTags (tagSplit: string list option): Tag list option =
        match tagSplit with
        | None -> None
        | Some tagSplit ->
            Some 
                [
                    for tag in tagSplit -> 
                        tag.Split('=') 
                        |> fun kvp -> 
                            match kvp.Length with
                            | 1                   -> {Key = kvp.[0]; Value = None}
                            | 2 when kvp.[1] = "" -> {Key = kvp.[0]; Value = None}
                            | 2                   -> {Key = kvp.[0]; Value = Some kvp.[1]}
                            | _                   -> {Key = "FAILURE"; Value = None}
                ]
    
    /// Takes the source extracted in messageSplit and creates a Source record from it
    let parseSource (sourceString: string option): Source option =
        if sourceString = None then None
        else
        let sourceString = sourceString.Value

        sourceString.Split('!').[0]
        |> fun nick ->
            (match nick with
            | "" -> stringTrimFirstIf(sourceString, '!')
            | _ -> sourceString.Replace(nick, "")
            , nick)
        |> fun (rest, nick) -> 
            let host = rest.Split('@').[1]
            (match host with
            | "" -> stringTrimLastIf(rest, '@')
            | _ -> rest.Replace(host, "")
            , nick, host)
        |> fun (rest, nick, host) -> 
            Some {Nick = nick; User = rest; Host = host}

    /// Parses the parameters of a message string from just the parameters part 
    let parseParameters (parametersString: string option): string list option =
        if parametersString.IsNone then None
        else
        let parametersString = parametersString.Value

        let paramsSplit = parametersString.Split(':') |> Array.where (fun x -> x <> "")

        match paramsSplit.Length with // Check how many trailing params it has
        | 0  -> None
        | 1 when paramsSplit.[0] = " " -> None // No params
        | 1 -> // Trailing params marker is optional
            let subSplit = paramsSplit.[0].TrimStart(' ').Split(' ') |> Array.where (fun x -> x <> "")
            match subSplit.Length with
            | 1 | 0 -> Some [ paramsSplit.[0] ] 
            | _ -> // More than one param found
                let primary = paramsSplit.[0].Replace(subSplit.[0], "").Split(' ') |> Array.where (fun x -> x <> "") |> Array.toList
                Some ([ subSplit.[0] ] @ primary)
        | _ -> // Found trialing params marker
            let primary = ((paramsSplit.[0].Split(' ') |> Array.where (fun x -> x <> "")) |> Array.toList)
            let secondary = parametersString.Replace(paramsSplit.[0], "") |> fun x -> stringTrimFirstIf (x, ':')
            match secondary with
            | "" -> Some primary
            | _ -> Some (primary @ [secondary])

    /// Uses regex to find the different groups of the IRC string message
    /// TODO: Look into using FParsec
    let messageSplit (message: string) =
        message
        |> fun message -> // Extract Tags
            let tags = matchRegexFirst message tagsRegex
            ( // Return Value
                match tags with
                | Some tags -> message.Replace(tags, "").TrimStart(' ')
                | None -> message
                , tags
            )
        |> fun (message, tags) -> // Extract Source
            let source = matchRegexFirst message sourceRegex
            ( // Return value
                match source with
                | Some source -> message.Replace(source, "").TrimStart(' ')
                | None -> message
                , tags, source
            )
        |> fun (message, tags, source) -> // Extract Command
            let command = extractString((matchRegexFirst message commandRegex), extractCommand)
            match command with
            | None -> (tags, source, None, None)
            | Some command -> 
                let verb = command.Split(' ').[0].TrimStart(' ')
                ( // Return Value
                    tags, 
                    source, 
                    Some verb,
                    match verb with
                    | "" -> None
                    | _ -> Some (command.Replace(verb, "").TrimStart(' '))
                )
        |> fun (tags, source, verb, parameters) -> // Digest all parts of the message
            { // Construct Message Record
                Tags = parseTags (extractList (tags, extractTags));
                Source = extractString (source, extractSource);
                Verb = verb;
                Params = parseParameters parameters
            }
