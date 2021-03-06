#load "../../../../.paket/load/netstandard2.0/FParsec.fsx"
#load "../../Utils/GeneralHelpers.fsx"

namespace FuncIRC

/// <summary>
/// FParsec implementation to parse an IRC message string into its constituent parts
/// </summary>
#if DEBUG
module MessageParserInternalsV2 =
#else
module internal MessageParserInternalsV2 =
#endif

    open FParsec
    open GeneralHelpers

    type UserState = unit
    type Parser<'t> = Parser<'t, UserState>

    // @aaa=bbb;ccc;example.com/ddd=eee :nick!ident@host.com PRIVMSG me :Hello
    
    /// <summary> 
    /// Parser to handle and convert unicode hex to string 
    /// Direct excerpt from FParsec tutorial
    /// </summary>
    let unicodeEscape: Parser<_> =
        let hex2int c = (int c &&& 15) + (int c >>> 6)*9

        pstring "u" >>. pipe4 hex hex hex hex (fun h3 h2 h1 h0 ->
            (hex2int h3)*4096 + (hex2int h2)*256 + (hex2int h1)*16 + hex2int h0
            |> char |> string
        )

    /// <summary> 
    /// Gets the value of the parser, returns the default value if the parser has a Failure result 
    /// </summary
    let getParserValue r =
        match r with
        | Success(result, _, _)   -> result
        | Failure(errorMsg, _, _) -> Unchecked.defaultof<'a>

    // # Helpers for default value when a parser gets a critical failure
    let optionalEmpty o p = p <|>% o
    let optionalEmptyList p = optionalEmpty [] p
    let optionalEmptyString p = optionalEmpty "" p
    // / Helpers for default value

    // # Tags Parsers
    /// <summary> Splits the individual tags by the = character if it is there </summary>
    let pSplitTags: Parser<_> = 
        let keyParser =  manyChars (noneOf "=;")
        let valueParser = pstring "=" >>. manyChars (noneOf ";") <|>% ""
        optional (skipString ";") >>. (keyParser .>>. valueParser)
    
    /// <summary> parses the tags only string into (key, value) list </summary>
    let splitAllTags: Parser<_> =
        many1 (notEmpty pSplitTags) <|> (pstring "" >>. preturn [])

    /// <summary> Find the whole tags string if it exists </summary>
    let tagsParser: Parser<_> = 
        pstring "@" >>. manyCharsTill (noneOf "") (lookAhead (pstring " ")) |> optionalEmptyString
    // / Tags Parsers

    // # Source Parsers
    /// <summary> Parses the nick part of a source segment if it exists </summary>
    let nickParser: Parser<_> = 
        optional (skipString ":") >>. (manyChars (noneOf "@!./")) .>>? notFollowedBy (anyOf "./")

    /// <summary> Parses the user part of a source segment if it exists </summary>
    let userParser: Parser<_> = 
        pstring "!" >>. manyCharsTill (noneOf "") (lookAhead (pstring "@"))

    /// <summary> Parses the host part of a source segment if it exists </summary>
    let hostParser: Parser<_> = 
        optional (skipString "@" <|> skipString ":") >>. manyChars (noneOf "") .>> eof

    /// <summary> Splits the source into (nick, user, hostname) </summary>
    let splitSource: Parser<_> =
        pipe3 
            ( attempt nickParser |> optionalEmptyString ) // Nick
            ( attempt userParser |> optionalEmptyString ) // User
            ( hostParser ) // Host
            ( fun a b c -> (a, b, c) )

    /// <summary> Parses source of message </summary>
    let sourceParser: Parser<_> = 
        optional (skipString " ") >>? (pstring ":" >>. manyCharsTill (noneOf "") (pstring " ")) |> optionalEmptyString
    // / Source Parsers

    // # Command Parsers
    /// <summary> Separates the leading parameters by space and the trailing parameter by : if it exists </summary>
    let getParameters: Parser<_> =
        let leadingParams  = optional (skipString " ") >>. sepBy (manyChars (noneOf " :")) (pstring " ")
        let trailingParams = optional (skipString ":") >>. manyChars (noneOf "")
        leadingParams .>>. trailingParams

    /// <summary> Takes the whole command string and splits it into verb and parameters </summary>
    let splitCommand: Parser<_> =
        pipe2
            ( manyChars <| noneOf " " ) // Verb
            ( getParameters ) // Params
            ( fun a b -> 
                (a, (fst b @+ snd b) @! "")
            )

    /// <summary> Parses command of message </summary>
    let commandParser: Parser<_> = 
        optional (skipString " ") >>. manyChars (noneOf "") .>> eof |> optionalEmptyString //many1Satisfy isCommandValid .>> eof |> optionalEmptyString
    // / Command Parsers

    /// <summary> Parses whole message into its constituent parts using FParsec </summary>
    let messageParser: Parser<_> = 
        pipe3 tagsParser sourceParser commandParser
            (fun tags source command -> 
                (
                    tags    |> run splitAllTags |> getParserValue,
                    source  |> run splitSource  |> getParserValue, 
                    command |> run splitCommand |> getParserValue
                )
            )

    /// <summary>
    /// Entry point to run the message parser. Returns the value of the parsing result.
    /// </summary>
    let runMessageParser s =
        run messageParser s |> getParserValue