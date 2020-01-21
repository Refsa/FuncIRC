namespace FuncIRC
#load "Utils.fsx"
#load "RegexHelpers.fsx"

module MessageParser =

    open Utils
    open RegexHelpers

    type Message =
        { Tags: string list option
          Source: string option
          Verb: string option
          Params: string list option }

    type Key =
        { Key: string
          Value: string option }

    let tagsRegex = @"(^@\S+)"
    let sourceRegex = @"(:\S+[@.]+\S+)"
    let commandRegex = @"\s?([A-Z]+.+)"

    // TODO: Currently doesnt work with .NET regex but works with Javascript/PHP implementation
    let messageSplitRegex = @"(^@\S+)?(:\S+[@.]+\S+)?\s([A-Z]+.+)?$"

    let extractRegexGroup (regex: string list option, group: int): string option =
        match regex with
        | Some regex -> Some(regex.[group].Trim(' '))
        | None -> None

    /// Prepares the tags part from a regex match
    let extractTagsFromRegex (regex: string list option): string list option =
        match regex with
        | Some regex -> Some((regex.[0].TrimStart('@').Trim(' ').Split(';')) |> Array.toList)
        | None -> None

    /// Prepares the source part from a regex match
    let extractSourceFromRegex (regex: string list option): string option =
        match regex with
        | Some regex -> Some(regex.[0].TrimStart(':').Trim(' '))
        | None -> None

    /// Prepares the command part from a regex match to a string
    let extractCommandFromRegex (regex: string list option): string option =
        match regex with
        | Some regex -> Some(regex.[0].Trim(' '))
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
            | _ -> matchRegex (message.Replace(tagsString, "")) sourceRegex
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
                | _ -> matchRegex (message.Replace(sourceString, "")) commandRegex
            | _ -> 
                match sourceString with // Check if there was a source
                | "" -> matchRegex (message.Replace(tagsString, "")) commandRegex
                | _ -> matchRegex (message.Replace(tagsString, "").Replace(sourceString, "")) commandRegex

        // Extract information from regex groups
        let tags = extractTagsFromRegex (tagsGroup)
        let source = extractSourceFromRegex (sourceGroup)
        let command = extractCommandFromRegex (commandGroup)

        // Prepare the information for execution
        let parsedMessage =
            { Tags = tags
              Source = source
              Verb = command
              Params = None }

        parsedMessage
