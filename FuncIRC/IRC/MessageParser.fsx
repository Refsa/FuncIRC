namespace FuncIRC
#load "IrcUtils.fsx"
#load "MessageTypes.fsx"
#load "MessageParserInternals.fsx"
#load "../Utils/RegexHelpers.fsx"
#load "../Utils/StringHelpers.fsx"
#load "../Utils/GeneralHelpers.fsx"

module MessageParser =
    open MessageParserInternals
    open MessageTypes
    open GeneralHelpers

    /// Uses regex to find the different groups of the IRC string message
    /// Parse individual parts of the groupings
    /// Total runtime is about 200 ticks (0.002ms)
    /// TODO: Look into using FParsec
    let parseMessageString (message: string) =
        let tags, source, verb, parameters = messageSplit message

        { // Construct Message Record
            Tags   = parseTags       (extractList   (tags, extractTags));
            Source = parseSource     (extractString (source, extractSource));
            Verb   = toVerb          (verb);
            Params = parseParameters (parameters)
        }
