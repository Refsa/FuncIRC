namespace FuncIRC
#load "../../../../.paket/load/netstandard2.0/FParsec.fsx"
#load "../../Utils/RegexHelpers.fsx"
#load "../../Utils/StringHelpers.fsx"
#load "../../Utils/GeneralHelpers.fsx"
#load "../../Utils/IrcUtils.fsx"

/// <summary>
/// FParsec implementation to parse an IRC message string into its constituent parts
/// </summary>
#if DEBUG
module MessageParserInternalsV2 =
#else
module internal MessageParserInternalsV2 =
#endif

    open FParsec
    open FParsec.CharParsers
    open FParsec.Primitives
    open IrcUtils
    open GeneralHelpers

    type UserState = unit
    type Parser<'t> = Parser<'t, UserState>

    // @aaa=bbb;ccc;example.com/ddd=eee :nick!ident@host.com PRIVMSG me :Hello
    
    let unicodeEscape: Parser<_> =
        let hex2int c = (int c &&& 15) + (int c >>> 6)*9

        pstring "u" >>. pipe4 hex hex hex hex (fun h3 h2 h1 h0 ->
            (hex2int h3)*4096 + (hex2int h2)*256 + (hex2int h1)*16 + hex2int h0
            |> char |> string
        )

    let getParserValue r =
        match r with
        | Success(result, _, _)   -> result
        | Failure(errorMsg, _, _) -> Unchecked.defaultof<'a>

    let optionalEmpty o p = p <|>% o
    let optionalEmptyList p = optionalEmpty [] p
    let optionalEmptyString p = optionalEmpty "" p

    let tagsMarker c = c = '@'
    let isTagsValid c = isLetter c || isDigit c || isAnyOf "/.=;" c

    let sourceMarker c = c = ':'
    let isSourceValid c = (isLetter c || isDigit c || isHex c || isAnyOf "/!.@" c) && isNoneOf [char "\u0000"; char "\u000d"; char "\u000a"; char "\u0008"] c

    let isCommandValid c = isLetter c || isDigit c //|| isNoneOf "\n\r\x00" c

    // # Tags Parsers
    /// <summary> Splits the individual tags by the = character if it is there </summary>
    let pSplitTags: Parser<_> = 
        tuple2 (manyChars <| noneOf "=") (optional <| skipString "=" >>. (manyChars <| noneOf ""))
    
    /// <summary> Runs the pSplitTags parser for every tag </summary>
    let splitTags (tags: string list) =
        match tags.Length with
        | 0 -> []
        | _ when tags.[0] = "" -> []
        | _ -> [ for tag in tags -> run pSplitTags tag |> getParserValue ]

    /// <summary> Splits all the tags by the ; character </summary>
    let splitAllTags = 
        sepBy (manyChars <| noneOf ";") (pstring ";") |>> splitTags .>> eof

    /// <summary> Entry point to parse tags, gives an empty list if no tags were found </summary>
    let tagParser: Parser<_> = 
        optional <| skipString "@" >>. splitAllTags |> optionalEmptyList

    /// <summary> Find the whole tags string if it exists </summary>
    let tagsParser: Parser<_> = many1Satisfy2 tagsMarker isTagsValid |> optionalEmptyString
    // / Tags Parsers

    // # Source Parsers
    /// Parses the nick part of a source segment if it exists
    let nickParser: Parser<_> = 
        //(pstring ":") >>. (manyChars <| noneOf "!@") .>> (attempt (pstring "!") <|> (pstring "@"))
        (optional <| skipString ":") >>. (manyChars <| noneOf "@!./") .>>? notFollowedBy (anyOf "./")

    /// Parses the user part of a source segment if it exists
    let userParser: Parser<_> = 
        pstring "!" >>. manyCharsTill (noneOf "") (lookAhead (pstring "@"))

    /// Parses the host part of a source segment if it exists
    let hostParser: Parser<_> = 
        (optional <| skipString "@") >>. (optional <| skipString ":") >>. (manyChars <| noneOf "" .>> eof)

    /// Splits the source into (nick, user, hostname)
    let splitSource: Parser<_> =
        pipe3 
            ( attempt <| nickParser |> optionalEmptyString ) // Nick
            ( attempt <| userParser |> optionalEmptyString ) // User
            ( hostParser ) // Host
            ( fun a b c -> (a, b, c) )

    /// Parses source of message
    let sourceParser: Parser<_> = 
        (optional <| skipString " ") >>? (pstring ":" >>. manyCharsTill (noneOf "") (pstring " ")) |> optionalEmptyString
    // / Source Parsers

    // # Command Parsers
    let getParameters: Parser<_> =
        (optional <| skipString " ") >>. sepBy (manyChars <| noneOf " :") (pstring " ") .>>. ((optional <| skipString ":") >>. (manyChars <| noneOf ""))

    let splitCommand: Parser<_> =
        pipe2
            ( manyChars <| noneOf " " ) // Verb
            ( getParameters ) // Params
            ( fun a b -> 
                let b, p = b
                let b = (b @+ p) @! ""
                (a, b)
            )

    /// Parses command of message
    let commandParser: Parser<_> = 
        optional <| skipString " " >>. manyChars (noneOf "") .>> eof |> optionalEmptyString //many1Satisfy isCommandValid .>> eof |> optionalEmptyString
    // / Command Parsers

    /// Parses whole message into its constituent parts using FParsec
    let messageParser: Parser<_> = 
        pipe3 tagsParser sourceParser commandParser 
            (fun a b c -> 
                (
                    a |> run tagParser |> getParserValue,
                    b |> run splitSource |> getParserValue, 
                    c |> run splitCommand |> getParserValue
                )
            )

    let runMessageParser s =
        run messageParser s |> getParserValue