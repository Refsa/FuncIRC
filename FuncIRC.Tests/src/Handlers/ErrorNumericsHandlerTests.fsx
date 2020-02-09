#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests
namespace NUnit
namespace NUnit.Framework

module ErrorNumericsHandlerTests =
    open FuncIRC.IRCClient
    open FuncIRC.MessageTypes
    open FuncIRC.ErrorNumericsHandlers

    let ircClientData(): IRCClient = IRCClient()

    let newVerbNameMessage verbName = Message.NewSimpleMessage (Some (Verb verbName)) None

//#region Error responses from server
    let verifyErrorResponse wantedResponse handler message (clientData: IRCClient) =
        let mutable errorResponse = ""
        clientData.ErrorNumericReceivedEvent
        |> Event.add (
            fun m -> errorResponse <- m
        )

        handler (message, clientData)

        wantedResponse = errorResponse

    [<Test>]
    let ``ERROR verb``() =
        ()

    [<Test>]
    let ``ERR_NEEDMOREPARAMS``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_NEEDMOREPARAMS")) (Some (toParameters [|"Nick"; "Command"; "Not enough parameters"|]))
        let wantedErrorResponse = "Command: Did not have enough parameters"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errNeedMoreParamsHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_ALREADYREGISTERED``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_ALREADYREGISTERED")) (Some (toParameters [|"Client"; "You may not reregister"|]))
        let wantedErrorResponse = "Already Registered: You have already registered with the server, cant change details at this time"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errAlreadyRegisteredHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_NONICKNAMEGIVEN``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_NONICKNAMEGIVEN")) (Some (toParameters [|"Client"; "no nickname given"|]))
        let wantedErrorResponse = "No Nickname was supplied to the NICK command"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errNoNicknameGivenHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_ERRONEUSNICKNAME``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_ERRONEUSNICKNAME")) (Some (toParameters [|"Client"; "somebannednick"; "Erroneus nickname"|]))
        let wantedErrorResponse = "Nickname [somebannednick] was not accepted by server: Erroneus Nickname"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errErroneusNicknameHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_NICKNAMEINUSE``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_NICKNAMEINUSE")) (Some (toParameters [|"Client"; "unavailablenick"; "Nickname is already in use"|]))
        let wantedErrorResponse = "Nickname [unavailablenick] is already in use on server"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errNicknameInUseHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_NICKCOLLISION``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_NICKCOLLISION")) (Some (toParameters [|"Client"; "collisionnick"; "something about colliding nicks"|]))
        let wantedErrorResponse = "Nickname [collisionnick] threw a nick collision response from server"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errNickCollisionHandler message clientData
        Assert.True (wasCorrectResponse)

    // # JOIN related
    [<Test>]
    let ``ERR_NOSUCHCHANNEL``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_NOSUCHCHANNEL")) (Some (toParameters [|"Client"; "#channel"; "No such channel"|]))
        let wantedErrorResponse = "[#channel] does not exist"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errNoSuchChannelHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_TOOMANYCHANNELS``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_TOOMANYCHANNELS")) (Some (toParameters [|"Client"; "#channel"; "you have joined too many channels"|]))
        let wantedErrorResponse = "[#channel] You have joined too many channels"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errTooManyChannelsHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_BADCHANNELKEY``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_BADCHANNELKEY")) (Some (toParameters [|"Client"; "#channel"; "Cannot join channel (+k)"|]))
        let wantedErrorResponse = "[#channel] cannot join channel, bad channel key"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errBadChannelKeyHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_BANNEDFROMCHAN``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_BANNEDFROMCHAN")) (Some (toParameters [|"Client"; "#channel"; "Cannot joint channel (+b)"|]))
        let wantedErrorResponse = "[#channel] you are banned from this channel"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errBannedFromChanHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_CHANNELISFULL``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_CHANNELISFULL")) (Some (toParameters [|"Client"; "#channel"; "Cannot join channel (+l)"|]))
        let wantedErrorResponse = "[#channel] is full, you cannot join it"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errChannelIsFullHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_INVITEONLYCHAN``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_INVITEONLYCHAN")) (Some (toParameters [|"Client"; "#channel"; "Cannot join channel (+i)"|]))
        let wantedErrorResponse = "[#channel] is invite only"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errInviteOnlyChanHandler message clientData
        Assert.True (wasCorrectResponse)
    // / JOIN related

    // # PRIVMSG related
    [<Test>]
    let ``ERR_NOSUCHNICK``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_NOSUCHNICK")) (Some (toParameters [|"Client"; "othernick"; "No such nick/channel"|]))
        let wantedErrorResponse = "[othernick] user does not exist in channel"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errNoSuchNickHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_NOSUCHSERVER``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_NOSUCHSERVER")) (Some (toParameters [|"Client"; "servername"; "No such server"|]))
        let wantedErrorResponse = "[servername] does not exist"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errNoSuchServerHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_CANNOTSENDTOCHAN``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_CANNOTSENDTOCHAN")) (Some (toParameters [|"Client"; "#channel"; "Cannot send to channel"|]))
        let wantedErrorResponse = "Cannot send to [#channel]"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errCannotSendToChanHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_TOOMANYTARGETS``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_TOOMANYTARGETS")) (Some (toParameters [|"Client"; "too many targets for PRIVMSG"|]))
        let wantedErrorResponse = "Too many targets for PRIVMSG"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errTooManyTargetsHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_NORECEIPIENT``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_NORECEIPIENT")) (Some (toParameters [|"Client"; "No receipient for PRIVMSG"|]))
        let wantedErrorResponse = "No receipient for PRIVMSG"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errNoReceipientHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_NOTEXTOTSEND``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_NOTEXTOTSEND")) (Some (toParameters [|"Client"; "No text to send"|]))
        let wantedErrorResponse = "No text to send in PRIVMSG"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errNoTextToSendHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_NOTOPLEVEL``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_NOTOPLEVEL")) (Some (toParameters [|"Client"; "No top level for message"|]))
        let wantedErrorResponse = "No top level for PRIVMSG"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errNoTopLevelHandler message clientData
        Assert.True (wasCorrectResponse)

    [<Test>]
    let ``ERR_WILDTOPLEVEL``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "ERR_WILDTOPLEVEL")) (Some (toParameters [|"Client"; "Wild top level for message"|]))
        let wantedErrorResponse = "Wild top level for PRIVMSG"

        let wasCorrectResponse = verifyErrorResponse wantedErrorResponse errWildTopLevelHandler message clientData
        Assert.True (wasCorrectResponse)
    // / PRIVMSG related

//#endregion Error responses from server