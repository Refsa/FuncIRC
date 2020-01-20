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
          Params: string list option } with

          member this.Print =
            printf "\n\tTags: "
            if this.Tags.IsSome then
                printf "%A" this.Tags.Value
            printf "\n\tSource: "
            if this.Source.IsSome then
                printf "%s" this.Source.Value
            printf "\n\tVerb: "
            if this.Verb.IsSome then
                printf "%s" this.Verb.Value
            printf "\n\tParams: "
            if this.Params.IsSome then
                this.Params.Value |> List.iter (fun a -> printf "%s " a)

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

    let extractTagsFromRegex (regex: string list option): string list option =
        match regex with
        | Some regex -> Some((regex.[0].TrimStart('@').Trim(' ').Split(';')) |> Array.toList)
        | None -> None

    let extractSourceFromRegex (regex: string list option): string option =
        match regex with
        | Some regex -> Some(regex.[0].TrimStart(':').Trim(' '))
        | None -> None

    let extractCommandFromRegex (regex: string list option): string option =
        match regex with
        | Some regex -> Some(regex.[0].Trim(' '))
        | None -> None

    let messageSplit (message: string) =
        // Use regex to find the different groups of the IRC string message
        let tagsGroup = matchRegex message tagsRegex
        let sourceGroup = matchRegex message sourceRegex
        let commandGroup = matchRegex message commandRegex

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
