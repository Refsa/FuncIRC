module Utils =
    let plaintextPort = 6667 // TCP
    let tlsPort = 6697 // TCP

    // Special handling of escape values
    let escapingValues = 
        [| 
            @"\:" // ;
            @"\s" // SPACE
            @"\\" // \
            @"\r" // CR Message Separator
            @"\n" // LF Message Separator
        |]

    let regularChannelMarker = "#"
    let localChannelMarker = "&"

    let tagsMarker = "@"
    let sourceMarker = ":"
    let parametersMarker = "*"

    let maxParamsSending = 15

    let tagsDelimiter = ";"
    let clientPrefix = "+"
    let endOfMessage = @" \r\n"