namespace FuncIRC
#load "Utils.fsx"
#load "RegexHelpers.fsx"
#load "StringHelpers.fsx"

module MessageParser =

    open Utils
    open RegexHelpers
    open StringHelpers

    type Message =
        { Tags: string list option
          Source: string option
          Verb: string option
          Params: string list option }

    type Key =
        { Key: string
          Value: string option }

    let tagsRegex = @"^(@\S+)"
    let sourceRegex = @"^(:[\S.]+)" //@"^(:\S+[@.]+\S+)"
    let commandRegex = @"^([a-zA-Z0-9]+.+)" //@"\s?([A-Z]+.+)"

    // TODO: Currently doesnt work with .NET regex but works with Javascript/PHP implementation
    let messageSplitRegex = @"(^@\S+)?(:\S+[@.]+\S+)?\s([A-Z]+.+)?$"

    let extractTags (tagsString: string) =
        ((tagsString.TrimStart('@').TrimStart(' ').Split(';')) |> Array.toList)

    let extractSource (sourceString: string) =
        (sourceString.TrimStart(':').TrimStart(' '))

    let extractCommand (commandString: string) =
        (commandString.TrimStart(' '))

    let extractRegexGroup (regex: string list option, group: int): string option =
        match regex with
        | Some regex -> Some (regex.[group].TrimStart(' '))
        | None -> None

    /// Prepares the tags part from a regex match
    let extractTagsFromRegex (regex: string list option): string list option =
        match regex with
        | Some regex -> Some (extractTags regex.[0])
        | None -> None

    /// Prepares the source part from a regex match
    let extractSourceFromRegex (regex: string list option): string option =
        match regex with
        | Some regex -> Some (extractSource regex.[0])
        | None -> None

    /// Prepares the command part from a regex match to a string
    let extractCommandFromRegex (regex: string list option): string option =
        match regex with
        | Some regex -> Some (extractCommand regex.[0])
        | None -> None

    /// Uses regex to find the different groups of the IRC string message
    /// TODO:
    ///     Refactor into a more functional approach not using regex
    ///     FParsec library is a good candidate
    let messageSplit (message: string) =
        // Find tags if there are any
        let tagsGroup = matchRegex message tagsRegex
        let tagsString = 
            match tagsGroup with
            | Some tg -> tg.[0]
            | None -> ""

        // Find source if there is one
        let sourceGroup = 
            match tagsString with // Remove tags from parsing if there were any
            | "" -> matchRegex message sourceRegex
            | _ -> matchRegex (message.Replace(tagsString, "").TrimStart(' ')) sourceRegex
        let sourceString =
            match sourceGroup with
            | Some sg -> sg.[0]
            | None -> ""
        
        // Find command if there is one
        let commandGroup = 
            match tagsString with // Check if there were any tags
            | "" ->
                match sourceString with // Check if there was a source
                | "" -> matchRegex message commandRegex
                | _ -> matchRegex (message.Replace(sourceString, "").TrimStart(' ')) commandRegex
            | _ -> 
                match sourceString with // Check if there was a source
                | "" -> matchRegex (message.Replace(tagsString, "").TrimStart(' ')) commandRegex
                | _ -> matchRegex (message.Replace(tagsString, "").Replace(sourceString, "").TrimStart(' ')) commandRegex
        let commandString = 
            match commandGroup with
            | Some cg -> extractCommand cg.[0]
            | None -> ""

        let verbString =
            match commandString with
            | "" -> ""
            | _ -> commandString.Split(' ').[0].TrimStart(' ')

        /// Message Split
        /// TODO: Should refactor this so it's more readable 
        let parameters =
            match verbString with
            | "" -> None
            | _ ->
                let paramsString = commandString.Replace (verbString, "") // Only parse params part of message
                let paramsSplit = paramsString.Split(':') // Find trailing params
                match paramsSplit.Length with // Check how many trailing params it has
                | 0 | 1 when paramsSplit.[0] = " " || paramsSplit.[0] = "" -> None // No params
                | 1 -> // Trailing params marker is optional
                    let subSplit = paramsSplit.[0].TrimStart(' ').Split(' ') |> Array.where (fun x -> x <> "")
                    match subSplit.Length with
                    | 1 | 0 -> Some [ paramsSplit.[0] ] 
                    | _ -> // More than one param found
                        let primary = paramsSplit.[0].Replace(subSplit.[0], "").Split(' ') |> Array.where (fun x -> x <> "") |> Array.toList
                        Some ([ subSplit.[0] ] @ primary)
                | _ -> // Found trialing params marker
                    let primary = ((paramsSplit.[0].Split(' ') |> Array.where (fun x -> x <> "")) |> Array.toList)
                    let secondary = paramsString.Replace(paramsSplit.[0], "") |> fun x -> stringTrimFirstIf (x, ':')
                    match secondary with
                    | "" -> Some primary
                    | _ -> Some (primary @ [secondary])

        // Extract information from regex groups
        let tags = extractTagsFromRegex (tagsGroup)
        let source = extractSourceFromRegex (sourceGroup)
        let verb =
            match verbString with
            | "" -> None
            | _ -> Some verbString

        // Prepare the information for execution
        let parsedMessage =
            { Tags = tags
              Source = source
              Verb = verb
              Params = parameters }

        parsedMessage
