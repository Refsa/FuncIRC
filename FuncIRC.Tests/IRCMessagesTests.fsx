#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework

open FuncIRC.IRCMessages
open FuncIRC.IRCClientData

module IRCMessagesTests =
    
    let createInvalidMessage message maxLength =
        let rec buildLongMessage (currentMessage: string) =
            if currentMessage.Length = maxLength then currentMessage
            else buildLongMessage (currentMessage + "_")
        buildLongMessage message

    [<Test>]
    let ``sendAwayMessage should add an out message if length is within bounds``() =
        let clientData = IRCClientData()

        let validMessage = "Away message that is not too long"
        let invalidMessage = createInvalidMessage "Away message that is too long " (clientData.GetServerInfo.MaxAwayLength + 1)

        sendAwayMessage clientData validMessage |> Assert.True

        let outboundMessage = clientData.GetOutboundMessages
        Assert.AreEqual ("AWAY " + validMessage, outboundMessage.Replace("\r\n", ""))

        sendAwayMessage clientData invalidMessage |> not |> Assert.True

        let outboundMessage = clientData.GetOutboundMessages

        Assert.AreEqual ("", outboundMessage)


    [<Test>]
    let ``sendQuitMessage should add an out message if length is within bounds``() =
        let clientData = IRCClientData()

        let validMessage = "Quit message that is not too long"
        let invalidMessage = createInvalidMessage "Quit message that is too long " 201

        sendQuitMessage clientData validMessage |> Assert.True

        let outboundMessage = clientData.GetOutboundMessages
        Assert.AreEqual ("QUIT " + validMessage, outboundMessage.Replace("\r\n", ""))

        sendQuitMessage clientData invalidMessage |> not |> Assert.True

        let outboundMessage = clientData.GetOutboundMessages

        Assert.AreEqual ("", outboundMessage)

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

    [<Test>]
    let ``sendRegistrationMessage should send and out message if parameters were correct or raise an exception``() =
        let clientData = IRCClientData()

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

    [<Test>]
    let ``sendKickMessage should add an outbound message if neither of kickUser or message is empty``() =
        let clientData = IRCClientData()

        sendKickMessage clientData "someuser" "somereason" |> Assert.True
        sendKickMessage clientData "" "somereason" |> Assert.False
        sendKickMessage clientData "someuser" "" |> Assert.False

    [<Test>]
    let ``sendTopicMessage should add an outbound message if the topic param is not empty``() =
        let clientData = IRCClientData()

        sendTopicMessage clientData "sometopic" |> Assert.True
        sendTopicMessage clientData "" |> Assert.False