namespace FuncIRC

/// General values related to IRCv3 protocol
module internal IrcUtils =

    let plaintextPort = 6667 // TCP
    let tlsPort = 6697 // TCP TLS

    // Special handling of escape values
    let escapingValues =
        [| @"\:" // ;
           @"\s" // SPACE
           @"\\" // \
           @"\r" // CR Message Separator
           @"\n" |]

    let crCharacter = char "\u000D"
    let lfCharacter = char "\u000A"
    let spaceCharacter = char "\u0020"
    let nulCharacter = char "\u0000"
    let bellCharacter = char "\u0008"

    let regularChannelMarker = "#"
    let localChannelMarker = "&"

    let tagsMarker = "@"
    let sourceMarker = ":"
    let parametersMarker = "*"

    let maxParamsSending = 15

    let tagsDelimiter = ";"
    let clientPrefix = "+"
    let endOfMessage = @" \r\n"
