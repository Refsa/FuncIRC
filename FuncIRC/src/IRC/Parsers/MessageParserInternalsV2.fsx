namespace FuncIRC
#load "../../../../.paket/load/netstandard2.0/FParsec.fsx"
#load "../../Utils/RegexHelpers.fsx"
#load "../../Utils/StringHelpers.fsx"
#load "../../Utils/GeneralHelpers.fsx"
#load "../../Utils/IrcUtils.fsx"

#if DEBUG
module MessageParserInternalsV2 =
#else
module internal MessageParserInternalsV2 =
#endif

    open FParsec
    open FParsec.CharParsers
    open FParsec.Primitives
    open IrcUtils

    type UserState = unit
    type Parser<'t> = Parser<'t, UserState>

    // @aaa=bbb;ccc;example.com/ddd=eee :nick!ident@host.com PRIVMSG me :Hello

    let getParserValue r =
        match r with
        | Success(result, _, _)   -> result
        | Failure(errorMsg, _, _) -> Unchecked.defaultof<'a>

    let optionalEmpty o p = p <|>% o
    let optionalEmptyList p = optionalEmpty [] p
    let optionalEmptyString p = optionalEmpty "" p

    let tagsMarker c = c = '@'
    let isTagsValid c = isLetter c || isDigit c || isAnyOf "/.=;\n\r\x00" c

    let sourceMarker c = c = ':'
    let isSourceValid c = isLetter c || isDigit c || isAnyOf "/!.@\n\r\x00" c

    let isCommandValid c = isLetter c || isDigit c || isNoneOf "\n\r\x00" c

    // # Tags Parsers
    /// <summary> Splits the individual tags by the = character if it is there </summary>
    let pSplitTags: Parser<_> = 
        tuple2 (manyChars <| noneOf "=") (optional <| skipString "=" >>. (manyChars <| noneOf ""))
    
    /// <summary> Runs the pSplitTags parser for every tag </summary>
    let splitTags (tags: string list) =
        [ for tag in tags -> run pSplitTags tag |> getParserValue ]

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
    /// Splits the source into (nick, user, hostname)
    let splitSource: Parser<_> =
        pipe3 
            (attempt ((pstring ":") >>. (manyChars <| noneOf "!") .>> (pstring "!")) |> optionalEmptyString)
            (attempt ((optional <| skipString ":") >>. (manyChars <| noneOf "@") .>> (pstring "@")) |> optionalEmptyString)
            ((optional <| skipString "@") >>. (optional <| skipString ":") >>. (manyChars <| noneOf "" .>> eof))
            (fun a b c ->
                (a, b, c)
            ) 

    /// Parses source of message
    let sourceParser: Parser<_> = 
        optional <| skipString " " >>. many1Satisfy2 sourceMarker isSourceValid |> optionalEmptyString
    // / Source Parsers

    /// Parses command of message
    let commandParser: Parser<_> = 
        optional <| skipString " " >>. many1Satisfy isCommandValid .>> eof |> optionalEmptyString
    
    /// Parses whole message into its constituent parts
    let messageParser: Parser<_> = 
        pipe3 tagsParser sourceParser commandParser 
            (fun a b c -> 
                (
                    a |> run tagParser |> getParserValue,
                    b |> run splitSource |> getParserValue, 
                    c
                )
            )

    // For reference, another way to do it but returns tuple combined with tuple and not a single tuple with all results
    //let messageParser: Parser<_> = (optionalEmpty tagsParser) .>>.
    //                               (optional (skipString " ") >>. optionalEmpty sourceParser) .>>.
    //                               (optional (skipString " ") >>. optionalEmpty commandParser)

    //let someParser: Parser<_> = regex @"^(@\S+)"