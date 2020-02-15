namespace FuncIRC
#load "../../Utils/IrcUtils.fsx"
#load "../Types/MessageTypes.fsx"
#load "../Parsers/MessageParserInternalsV2.fsx"
#load "../../Utils/GeneralHelpers.fsx"
#load "../../Utils/StringHelpers.fsx"

module MessageParserV2 =
    open MessageParserInternalsV2
    open MessageTypes
    open GeneralHelpers
    open StringHelpers

    /// <summary>
    /// FParsec version of message string parser
    /// </summary>
    let parseMessageString (message: string): Message =
        let parserResult = runMessageParser message
        let tags, source, command = parserResult

        let tags = 
            match tags.Length with
            | 0 -> None
            | _ -> Some ([ for tag in tags -> 
                               let k, v = tag
                               {Key = k; Value = stringOptionFromString v} ])

        let source = 
            let n, u, h = source
            match (n, h) with 
            | ("", "") -> None
            | _ ->
                Some ({ Nick = stringOptionFromString n
                        User = stringOptionFromString u
                        Host = stringOptionFromString h })

        let verb, parameters = command
        let verb = 
            match verb with
            | "" -> None
            | _  -> Some (Verb verb)

        let parameters = 
            match parameters.Length with
            | 0 -> None
            | _ -> Some (Parameters [| for p in parameters -> Parameter p |])

        {
            Tags = tags
            Source = source
            Verb = verb
            Params = parameters
        }
