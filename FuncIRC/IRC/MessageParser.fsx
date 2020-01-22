namespace FuncIRC
#load "IrcUtils.fsx"
#load "../Utils/RegexHelpers.fsx"
#load "../Utils/StringHelpers.fsx"
#load "../Utils/GeneralHelpers.fsx"

module MessageParser =

    open IrcUtils
    open RegexHelpers
    open StringHelpers
    open GeneralHelpers

    type Tag =
        { Key: string
          Value: string option }

    type Verb = Verb of string

    type Parameter = Parameter of string
    type Parameters = Parameters of Parameter list

    type Source =
        { Nick: string option
          User: string option
          Host: string option }

    type Message =
        { Tags: Tag list option
          Source: Source option
          Verb: Verb option
          Params: Parameters option }

    /// Takes a string option and transforms it to a Verb type
    let toVerb input =
        match input with
        | Some input -> Some (Verb input)
        | None -> None

    /// transforms a collection of strings into a Parameters type
    let toParameters input =
        Parameters [ for s in input -> Parameter s ]

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
                            | 1                   -> {Key = kvp.[0]; Value = None}
                            | 2                   -> {Key = kvp.[0]; Value = Some kvp.[1]}
                            | _                   -> {Key = "FAILURE"; Value = None}
                ]
    
    /// Takes the source extracted in messageSplit and creates a Source record from it
    /// TODO: Look into using FParsec
    let parseSource (sourceString: string option): Source option =
        if sourceString = None then None
        else
        let sourceString = sourceString.Value

        let nickSplit = sourceString.Split('!') |> Array.where (fun x -> x <> "")
        let hostSplit = sourceString.Split('@') |> Array.where (fun x -> x <> "")

        let noNickSplit = Array.length nickSplit = 1
        let noHostSplit = Array.length hostSplit = 1

        match sourceString with
        | ss when noNickSplit && noNickSplit = noHostSplit -> // @ and ! was not present, it's either Host or Nick
            match ss with
            | ss when ss.IndexOf('.') <> -1 -> // Host
                Some {Nick = None; User = None; Host = Some ss}
            | _ -> // Nick
                Some {Nick = Some ss; User = None; Host = None}
        | _ when noNickSplit -> // User and Host
            Some {Nick = None; User = Some (hostSplit.[0].Trim('!')); Host = Some (hostSplit.[1])}
        | _ when noHostSplit -> // Nick and User
            Some {Nick = Some nickSplit.[0]; User = Some nickSplit.[1]; Host = None}
        | _ -> // Nick, User and Host
            Some
                {
                    Nick = Some nickSplit.[0];
                    User = Some (nickSplit.[1].Replace(hostSplit.[1], "").Trim('@'));
                    Host = Some hostSplit.[1];
                }

    /// Parses the parameters of a message string from just the parameters part
    /// TODO: Look into using FParsec
    let parseParameters (parametersString: string option): Parameters option =
        if parametersString.IsNone then None
        else
        let parametersString = parametersString.Value

        let paramsSplit = parametersString.Split(':') |> Array.where (fun x -> x <> "")

        match paramsSplit.Length with // Check how many trailing params it has
        | 0  -> None // No Params
        | 1 when paramsSplit.[0] = " " -> None // No params
        | 1 -> // Trailing params marker is optional
            let subSplit = paramsSplit.[0].TrimStart(' ').Split(' ') |> Array.where (fun x -> x <> "")
            match subSplit.Length with
            | 1 | 0 -> Some (Parameters [ Parameter paramsSplit.[0] ])
            | _ -> // More than one param found
                let primary = paramsSplit.[0].Replace(subSplit.[0], "").Split(' ') |> Array.where (fun x -> x <> "") |> Array.toList
                
                Some (toParameters ([ subSplit.[0] ] @ primary))
        | _ -> // Found trialing params marker
            let primary = ((paramsSplit.[0].Split(' ') |> Array.where (fun x -> x <> "")) |> Array.toList)
            let secondary = parametersString.Replace(paramsSplit.[0], "") |> fun x -> stringTrimFirstIf (x, ':')

            match secondary with
            | "" -> Some (toParameters primary)
            | _ -> Some (toParameters (primary @ [secondary]))

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
