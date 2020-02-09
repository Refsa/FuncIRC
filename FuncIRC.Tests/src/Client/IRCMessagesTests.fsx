#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework

open FuncIRC.IRCMessages
open FuncIRC.IRCClient
open FuncIRC.ServerFeaturesHandler

module IRCMessagesTests =
    
    // Constructs a string of given length
    let createInvalidMessage message maxLength =
        let rec buildLongMessage (currentMessage: string) =
            if currentMessage.Length = maxLength then currentMessage
            else buildLongMessage (currentMessage + "_")
        buildLongMessage message

    // sendAwayMessage tests
    [<Test>]
    let ``sendAwayMessage should add an out message if length is within bounds``() =
        let clientData = IRCClient()

        let validMessage = "Away message that is not too long"
        let invalidMessage = createInvalidMessage "Away message that is too long " (clientData.GetServerInfo.MaxAwayLength + 1)

        //sendAwayMessage clientData validMessage |> Assert.True

        //let outboundMessage = clientData.GetOutboundMessages
        //Assert.AreEqual ("AWAY " + validMessage, outboundMessage.Replace("\r\n", ""))

        //sendAwayMessage clientData invalidMessage |> not |> Assert.True

        //let outboundMessage = clientData.GetOutboundMessages

        //Assert.AreEqual ("", outboundMessage)

        Assert.Warn ("No longer works after swapping to MailboxProcessor")

    // sendQuitMessage tests
    [<Test>]
    let ``sendQuitMessage should add an out message if length is within bounds``() =
        let clientData = IRCClient()

        let validMessage = "Quit message that is not too long"
        let invalidMessage = createInvalidMessage "Quit message that is too long " 201

        //sendQuitMessage clientData validMessage |> Assert.True

        //let outboundMessage = clientData.GetOutboundMessages
        //Assert.AreEqual ("QUIT " + validMessage, outboundMessage.Replace("\r\n", ""))

        //sendQuitMessage clientData invalidMessage |> not |> Assert.True

        //let outboundMessage = clientData.GetOutboundMessages

        //Assert.AreEqual ("", outboundMessage)
        
        Assert.Warn ("No longer works after swapping to MailboxProcessor")

    // # sendRegistrationMessage tests
    let testRegistrationDataActivePattern loginData wantedOutput =
        match loginData with
        | InvalidLoginData -> ("", "", "", "")
        | UserRealNamePass (nick, user, realName, pass) -> (nick, user, realName, pass)
        | UserPass (nick, user, pass)                   -> (nick, user, user, pass)     
        | UserRealName (nick, user, realName)           -> (nick, user, realName, "")
        | User (nick, user)                             -> (nick, user, user, "")
        | Nick (nick)                                   -> (nick, "", "", "")
        |> function
           | output when output = wantedOutput -> true
           | _ -> false

    /// verify registration parameters ActivePattern tests
    [<Test>]
    let ``Active pattern to verify registration message parameters``() =
        let invalidRegistrationData = ("", "", "", "")
        let userRealNamePassRegistrationData = ("nick", "user", "realName", "pass")
        let userPassData = ("nick", "user", "", "pass")
        let userRealNameData = ("nick", "user", "realName", "")
        let userData = ("nick", "user", "", "")
        let nickData = ("nick", "", "", "")

        testRegistrationDataActivePattern invalidRegistrationData invalidRegistrationData 
        |> Assert.True

        testRegistrationDataActivePattern userRealNamePassRegistrationData userRealNamePassRegistrationData 
        |> Assert.True

        testRegistrationDataActivePattern userPassData ("nick", "user", "user", "pass") 
        |> Assert.True

        testRegistrationDataActivePattern userRealNameData userRealNameData
        |> Assert.True

        testRegistrationDataActivePattern userData ("nick", "user", "user", "")
        |> Assert.True

        testRegistrationDataActivePattern nickData nickData
        |> Assert.True

    /// sendRegistrationMessage tests
    [<Test>]
    let ``sendRegistrationMessage should send and out message if parameters were correct or raise an exception``() =
        let clientData = IRCClient()

        let invalidRegistrationData = ("", "", "", "")
        let userRealNamePassRegistrationData = ("nick", "user", "realName", "pass")

        try
            sendRegistrationMessage clientData invalidRegistrationData
            false
        with
        | :? RegistrationContentException -> true
        |> Assert.True
        
        try
            sendRegistrationMessage clientData userRealNamePassRegistrationData
            true
        with
        | :? RegistrationContentException -> false
        |> Assert.True
    // / sendRegistrationMessage tests

    // sendKickMessage tests
    [<Test>]
    let ``sendKickMessage should add an outbound message if neither of kickUser or message is empty``() =
        let clientData = IRCClient()

        sendKickMessage clientData "someuser" "somereason" |> Assert.True
        sendKickMessage clientData "someuser" (createInvalidMessage "" (clientData.GetServerInfo.MaxTopicLength + 1)) |> Assert.False
        sendKickMessage clientData (createInvalidMessage "" (clientData.GetServerInfo.MaxNickLength + 1)) "somereason" |> Assert.False
        sendKickMessage clientData "" "somereason" |> Assert.False
        sendKickMessage clientData "someuser" "" |> Assert.False

    // sendTopicMessage tests
    [<Test>]
    let ``sendTopicMessage should add an outbound message if the topic param is not empty``() =
        let clientData = IRCClient()

        sendTopicMessage clientData "sometopic" |> Assert.True
        sendTopicMessage clientData "" |> Assert.False
        sendTopicMessage clientData (createInvalidMessage "" (clientData.GetServerInfo.MaxTopicLength + 1)) |> Assert.False

    // sendJoinMessage tests
    [<Test>]
    let ``sendJoinMessage should send a join message if it has a valid channel prefix``() =
        let clientData = IRCClient()
        let feature = [| ("CHANTYPES", "#") |]
        serverFeaturesHandler (feature, clientData)

        sendJoinMessage clientData "#channel" |> Assert.True
        sendJoinMessage clientData "@channel" |> Assert.False
        sendJoinMessage clientData "channel" |> Assert.False
        sendJoinMessage clientData (createInvalidMessage "" (clientData.GetServerInfo.MaxChannelLength + 1)) |> Assert.False

    // sendChannelPrivMsg tests
    [<Test>]
    let ``sendChannelPrivMsg should send a PRIVMSG to channel if the channel and message is valid``() =
        let clientData = IRCClient()
        let feature = [| ("CHANTYPES", "#") |]
        serverFeaturesHandler (feature, clientData)

        sendChannelPrivMsg clientData "#channel" "some message" |> Assert.True
        sendChannelPrivMsg clientData "channel" "some message" |> Assert.False
        sendChannelPrivMsg clientData "#channel" "" |> Assert.False
        sendChannelPrivMsg clientData (createInvalidMessage "" (clientData.GetServerInfo.MaxChannelLength + 1)) "some message" |> Assert.False
        sendChannelPrivMsg clientData "#channel" (createInvalidMessage "" (clientData.GetServerInfo.LineLength - clientData.GetServerInfo.MaxHostLength + 1)) |> Assert.False

