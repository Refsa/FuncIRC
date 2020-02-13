namespace FuncIRC
#load "../../Utils/IrcUtils.fsx"
#load "../Types/MessageTypes.fsx"
#load "../Parsers/MessageParserInternals.fsx"
#load "../../Utils/GeneralHelpers.fsx"

module MessageParser =
    open MessageParserInternals
    open MessageTypes
    open GeneralHelpers

    /// <summary>
    /// Uses regex to find the different groups of the IRC string message
    /// Parse individual parts of the groupings
    /// Total runtime is about 200 ticks (0.002ms)
    /// TODO: Look into using FParsec
    /// </summary>
    let parseMessageString (message: string) =
        let tags, source, verb, parameters = messageSplit message

        { // Construct Message Record
            Tags   = parseTags       (extractList   (tags, trimAndSplitTagsString));
            Source = parseSource     (extractString (source, trimSourceString));
            Verb   = toVerb          (verb);
            Params = parseParameters (parameters)
        }
