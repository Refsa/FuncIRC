namespace FuncIRC
#load "IrcUtils.fsx"
#load "MessageTypes.fsx"
#load "../Utils/RegexHelpers.fsx"
#load "../Utils/StringHelpers.fsx"
#load "../Utils/GeneralHelpers.fsx"

module MessageParser =
    open GeneralHelpers
    open IrcUtils
    open MessageTypes
    open RegexHelpers
    open StringHelpers

    let tagsRegex = @"^(@\S+)"
    let sourceRegex = @"^(:[\S.]+)"
    let commandRegex = @"^([a-zA-Z0-9]+.+)"

    /// Trims the tags string of extranous characters and splits it with ';' character
    let extractTags (tagsString: string) =
        ((tagsString.TrimStart('@').TrimStart(' ').Split(';')) |> Array.toList)

    /// Trims the source string of extraneous characters
    let extractSource (sourceString: string) =
        (sourceString.TrimStart(':').TrimStart(' '))

    /// Trims the command string of extraneous characters
    let extractCommand (commandString: string) =
        (commandString.TrimStart(' '))

    /// Tages a string collection length to check if it's valid, contains key or key and value
    let (|IsTagKeyOnly|IsTagKeyValue|IsTagNone|) keyLen =
        match keyLen with
        | 1 -> IsTagKeyOnly
        | 2 -> IsTagKeyValue
        | _ -> IsTagNone

    /// Takes the list of tags extracted in messageSplit and creates a list of Tag Records from them
    let parseTags (tagSplit: string list option): Tag list option =
        match tagSplit with
        | None -> None
        | Some tagSplit ->
            Some 
                [
                    for tag in tagSplit -> 
                        tag.Split('=') 
                        |> Array.where (fun x -> x <> "")
                        |> fun kvp -> 
                            match kvp.Length with
                            | IsTagKeyOnly  -> {Key = kvp.[0];   Value = None}
                            | IsTagKeyValue -> {Key = kvp.[0];   Value = Some kvp.[1]}
                            | IsTagNone     -> {Key = "FAILURE"; Value = None}
                ]

    /// Takes the nick split, host split and if the source string has 
    ///  punctiation marks to find the correct combination of nick, user and host
    let (|IsNick|IsHost|IsNickUser|IsUserHost|IsNickHost|IsNickUserHost|) (nickSplit, hostSplit, hasPunct) =
        let nickSingle = Array.length nickSplit = 1
        let hostSingle = Array.length hostSplit = 1
           
        match () with
        | _ when nickSingle && hostSingle && hasPunct -> IsHost
        | _ when nickSingle && hostSingle -> IsNick
        | _ when nickSingle && not hostSingle -> IsNickHost
        | _ when hostSingle && not nickSingle -> IsNickUser
        | _ when not hostSingle && not nickSingle -> IsNickUserHost
        | _ -> IsUserHost

    /// Takes the source extracted in messageSplit and creates a Source record from it
    /// TODO: Look into using FParsec
    let parseSource (sourceString: string option): Source option =
        if sourceString = None then None
        else
        let sourceString = sourceString.Value

        let nickSplit = arrayRemove (sourceString.Split('!')) stringIsEmpty
        let hostSplit = arrayRemove (sourceString.Split('@')) stringIsEmpty

        match (nickSplit, hostSplit, (sourceString.IndexOf('.') <> -1)) with
        | IsHost         -> Some {Nick = None;               User = None;                           Host = Some sourceString}
        | IsNick         -> Some {Nick = Some sourceString;  User = None;                           Host = None}
        | IsUserHost     -> Some {Nick = None;               User = Some (hostSplit.[0].Trim('!')); Host = Some (hostSplit.[1])}
        | IsNickUser     -> Some {Nick = Some nickSplit.[0]; User = Some nickSplit.[1];             Host = None}
        | IsNickHost     -> Some {Nick = Some hostSplit.[0]; User = None;                           Host = Some hostSplit.[1]}
        | IsNickUserHost -> 
            Some
                {
                    Nick = Some nickSplit.[0];
                    User = Some (nickSplit.[1].Replace(hostSplit.[1], "").Trim('@'));
                    Host = Some hostSplit.[1];
                }

    /// Checks the length of the split parameters string to find out if it has trailing parameters or not
    let (|NoParams|HasTrailing|NoTrailing|) (paramSplit: string array) =
        match paramSplit.Length with
        | 0                  -> NoParams
        | 1 when paramSplit.[0] = " " -> NoTrailing
        | 1                  -> NoTrailing
        | _                  -> HasTrailing

    /// Picks out the params when there were no trailing marker
    let getParamsNoTrailing (paramsSplit: string array) =
        let subSplit = arrayRemove (paramsSplit.[0].TrimStart(' ').Split(' ')) stringIsEmpty
        match subSplit.Length with
        | 1 | 0 -> Some (Parameters [ Parameter paramsSplit.[0] ])
        | _ -> // More than one param found
            let primary = arrayRemove (paramsSplit.[0].Replace(subSplit.[0], "").Split(' ')) stringIsEmpty |> Array.toList
            Some (toParameters ([ subSplit.[0] ] @ primary))

    /// Picks out the parameters when a trailing marker was found
    let getParamsWithTrailing (paramsSplit: string array, parametersString: string) = 
        let primary = arrayRemove (paramsSplit.[0].Split(' ')) stringIsEmpty |> Array.toList
        let secondary = parametersString.Replace(paramsSplit.[0], "") |> fun x -> stringTrimFirstIf (x, ':')

        match secondary with
        | "" -> Some (toParameters primary)
        | _ -> Some (toParameters (primary @ [secondary]))

    /// Parses the parameters of a message string from just the parameters part
    /// TODO: Look into using FParsec
    let parseParameters (parametersString: string option): Parameters option =
        if parametersString.IsNone then None
        else
        let parametersString = parametersString.Value

        let paramsSplit = arrayRemove (parametersString.Split(':')) stringIsEmpty

        match paramsSplit with // Check how many trailing params it has
        | NoParams   -> None
        | NoTrailing -> getParamsNoTrailing paramsSplit
        | HasTrailing -> getParamsWithTrailing (paramsSplit, parametersString)
            

    /// Uses regex to find the different groups of the IRC string message
    /// TODO: Look into using FParsec
    let messageSplit (message: string) =
        message
        |> fun rest -> // Extract Tags
            let tags = matchRegexFirst rest tagsRegex
            ( // Return Value
                match tags with
                | Some tags -> rest.Replace(tags, "").TrimStart(' ')
                | None -> rest
                , tags
            )
        |> fun (rest, tags) -> // Extract Source
            let source = matchRegexFirst rest sourceRegex
            ( // Return value
                match source with
                | Some source -> rest.Replace(source, "").TrimStart(' ')
                | None -> rest
                , tags, source
            )
        |> fun (rest, tags, source) -> // Extract Command (Verb and Params)
            let command = extractString((matchRegexFirst rest commandRegex), extractCommand)
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
                Source = parseSource (extractString (source, extractSource));
                Verb = toVerb verb;
                Params = parseParameters parameters
            }
