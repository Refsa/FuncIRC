#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests
namespace NUnit
namespace NUnit.Framework

module MessageHandlerTests =
    open System
    open FuncIRC.IRCClientData
    open FuncIRC.MessageTypes
    open FuncIRC.IRCInformation
    open FuncIRC.MessageHandlers
    open FuncIRC.GeneralHelpers

    let ircClientData(): IRCClientData = IRCClientData()

    let newVerbNameMessage verbName = Message.NewSimpleMessage (Some (Verb verbName)) None

    [<Test>]
    let ``PING verb handler should add an outbound message with the verb PONG``() =
        let clientData = ircClientData()
        let message = newVerbNameMessage "PING"

        pongMessageHandler (message, clientData)

        let outboundMessages = clientData.GetOutboundMessages.Replace("\n", "").Split('\r') |> arrayRemove <| stringIsEmpty
        Assert.AreEqual (outboundMessages.Length, 1)
        Assert.AreEqual (outboundMessages.[0], "PONG")

    [<Test>]
    let ``RPL_WELCOME handler should do nothing``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"nick"; "Welcome to the Refsa IRC Network testnick!testuser@127.0.0.1"|])
        let verb = Some (Verb "RPL_WELCOME")
        let message = Message.NewSimpleMessage verb parameters

        rplWelcomeHandler (message, clientData)

        Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    [<Test>]
    let ``RPL_YOURHOST handler should do nothing``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "Your host is 127.0.0.1, running version InspIRCd-3"|])
        let verb = Some (Verb "RPL_YOURHOST")
        let message = Message.NewSimpleMessage verb parameters

        rplYourHostHandler (message, clientData)

        Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    [<Test>]
    let ``RPL_CREATED handler should update server info in IRCClientData with creation date``() =
        let clientData = ircClientData()
        let testParams = "This server was created 23:25:21 Jan 24 2020"
        let parameters = Some (toParameters [|"Nick"; testParams|])
        let verb = Some (Verb "RPL_CREATED")
        let message = Message.NewSimpleMessage verb parameters

        rplCreatedHandler (message, clientData)

        let serverInfo = clientData.GetServerInfo
        Assert.AreNotEqual (serverInfo, default_IRCServerInfo)
        Assert.AreEqual (serverInfo, {default_IRCServerInfo with Created = DateTime.Parse("23:25:21 Jan 24 2020");})

    [<Test>]
    let ``RPL_MYINFO handler should do nothing``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "127.0.0.1"; "InspIRCd-3"; "iosw"; "biklmnopstv"; "bklov"|])
        let verb = Some (Verb "RPL_MYINFO")
        let message = Message.NewSimpleMessage verb parameters

        rplMyInfoHandler (message, clientData)

        Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    [<Test>]
    let ``RPL_ISUPPORT handler should respond with everything except trailing params``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "AWAYLEN=200"; "CASEMAPPING=ascii"; "CHANLIMIT=#:20"; "CHANMODES=b,k,l,imnpst"; "CHANNELLEN=64"; "CHANTYPES=#"; "ELIST=CMNTU"; "HOSTLEN=64"; "KEYLEN=32"; "KICKLEN=255"; "LINELEN=512"; "MAXLIST=b:100"; "are supported by this server"|])
        let verb = Some (Verb "RPL_ISUPPORT")
        let message = Message.NewSimpleMessage verb parameters

        let wantedFeatures = 
            Features [| 
                        ("AWAYLEN", "200"); ("CASEMAPPING", "ascii"); ("CHANLIMIT", "#:20"); 
                        ("CHANMODES", "b,k,l,imnpst"); ("CHANNELLEN", "64"); ("CHANTYPES", "#"); 
                        ("ELIST", "CMNTU"); ("HOSTLEN", "64"); ("KEYLEN", "32"); ("KICKLEN", "255"); 
                        ("LINELEN", "512"); ("MAXLIST", "b:100") 
                    |]

        rplISupportHandler (message, clientData)

        Assert.AreEqual (wantedFeatures.Value.Length, clientData.GetServerFeatures.Length)
        let serverFeatureContentEquals = Array.forall2 (fun a b -> a=b) wantedFeatures.Value clientData.GetServerFeatures

        if not serverFeatureContentEquals then
            clientData.GetServerFeatures
            |> Array.iter (fun (k, v) -> printfn "%s=%s" k v)

        Assert.True (serverFeatureContentEquals)

    [<Test>]
    let ``RPL_LUSERCLIENT handler should do nothing for now``() =
        let clientData = ircClientData()
        let testParams = "There are 0 users and 0 invisible on 1 servers"
        let parameters = Some (toParameters [|"Nick"; testParams|])
        let verb = Some (Verb "RPL_LUSERCLIENT")
        let message = Message.NewSimpleMessage verb parameters

        rplLUserClientHandler (message, clientData)

        Assert.Warn ("Uncertain about how RPL_LUSERCLIENT should be handled")
        Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    [<Test>]
    let ``RPL_LUSERUNKNOWN handler should do nothing for now``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "1"; "unknown connections"|])
        let verb = Some (Verb "RPL_LUSERUNKNOWN")
        let message = Message.NewSimpleMessage verb parameters

        rplLUserUnknownHandler (message, clientData)

        Assert.Warn ("Uncertain about how RPL_LUSERUNKNOWN should be handled")
        Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    [<Test>]
    let ``RPL_LUSERCHANNELS handler should do nothing for now``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "0"; "channels formed"|])
        let verb = Some (Verb "RPL_LUSERCHANNELS")
        let message = Message.NewSimpleMessage verb parameters

        rplLUserChannelsHandler (message, clientData)

        Assert.Warn ("Uncertain about how RPL_LUSERCHANNELS should be handled")
        Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    [<Test>]
    let ``RPL_LUSERME handler should do nothing for now``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "I have 0 clients and 0 servers"|])
        let verb = Some (Verb "RPL_LUSERME")
        let message = Message.NewSimpleMessage verb parameters

        rplLUserMeHandler (message, clientData)

        Assert.Warn ("Uncertain about how RPL_LUSERME should be handled")
        Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    [<Test>]
    let ``RPL_LOCALUSERS handler should update server info on IRCClientData with local users info``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "Current local users: 1  Max: 10"|])
        let verb = Some (Verb "RPL_LOCALUSERS")
        let message = Message.NewSimpleMessage verb parameters

        rplLocalUsersHandler (message, clientData)

        let wantedServerInfo = { default_IRCServerInfo with LocalUserInfo = (1, 10) }

        Assert.AreNotEqual (clientData.GetServerInfo, default_IRCServerInfo)
        Assert.AreEqual (clientData.GetServerInfo, wantedServerInfo)

    [<Test>]
    let ``RPL_GLOBALUSERS handler should update server info on IRCClientData with global users info``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "Current global users: 1  Max: 10"|])
        let verb = Some (Verb "RPL_GLOBALUSERS")
        let message = Message.NewSimpleMessage verb parameters

        rplGlobalUsersHandler (message, clientData)

        let wantedServerInfo = { default_IRCServerInfo with GlobalUserInfo = (1, 10) }

        Assert.AreNotEqual (clientData.GetServerInfo, default_IRCServerInfo)
        Assert.AreEqual (clientData.GetServerInfo, wantedServerInfo)

//#region MOTD handler tests
    let motdContents =
        [
            "-  _____                        _____   _____    _____      _";
            "- |_   _|                      |_   _| |  __ \  / ____|    | |";
            "-   | |    _ __    ___   _ __    | |   | |__) || |       __| |";
            "-   | |   | '_ \  / __| | '_ \   | |   |  _  / | |      / _` |";
            "-  _| |_  | | | | \__ \ | |_) | _| |_  | | \ \ | |____ | (_| |";
            "- |_____| |_| |_| |___/ | .__/ |_____| |_|  \_\ \_____| \__,_|";
            "-     __________________| |_______________________________";
            "-    |__________________|_|_______________________________|";
        ]

    [<Test>]
    let ``RPL_MOTDSTART handler should do nothing``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "127.0.0.1 message of the day"|])
        let verb = Some (Verb "RPL_MOTDSTART")
        let message = Message.NewSimpleMessage verb parameters

        rplMotdStartHandler (message, clientData)

        Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    [<Test>]
    let ``RPL_MOTD handler should update MOTD content of IRCClientData with all the received MOTD message lines``() =
        let clientData = ircClientData()
        let verb = Some (Verb "RPL_MOTD")

        motdContents
        |> List.iter (
            fun mc ->
                let parameters = Some (toParameters [|"Nick"; mc|])
                let message = Message.NewSimpleMessage verb parameters
                rplMotdHandler (message, clientData)
        )

        Assert.AreEqual (motdContents.Length, clientData.GetServerMOTD.Length)
        let motdContentEqual = List.forall2 (fun a b -> a=b) clientData.GetServerMOTD motdContents

        if not motdContentEqual then
            clientData.GetServerMOTD
            |> List.iter (fun ml -> printfn "%s" ml)

        Assert.True (motdContentEqual)

    [<Test>]
    let ``RPL_ENDOFMOTD handler should do nothing``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "End of message of the day."|])
        let verb = Some (Verb "RPL_ENDOFMOTD")
        let message = Message.NewSimpleMessage verb parameters

        rplEndOfMotdHandler (message, clientData)

        Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)
//#endregion MOTD handler tests

//#region Channel messages
    let ``RPL_ENDOFNAMES``(clientData, usersInChannel) =
        let parameters = Some (toParameters [|"Nick"; "#channel"; "End of /NAMES list"|])
        let verb = Some (Verb "RPL_ENDOFMOTD")
        let message = Message.NewSimpleMessage verb parameters

        rplEndOfNamesHandler (message, clientData)

        let channelInfo = clientData.GetChannelInfo "#channel"
        Assert.True (channelInfo.IsSome)

        let channelInfo = channelInfo.Value
        let channelUsersEqual = Array.forall2 (fun a b -> a=b) channelInfo.Users usersInChannel

        Assert.AreEqual ("#channel", channelInfo.Name)
        Assert.AreEqual (usersInChannel.Length, channelInfo.UserCount)
        Assert.AreEqual ("Public", channelInfo.Status)
        Assert.True (channelUsersEqual, "The users in channelInfo were not the same as the input")

    [<Test>]
    let ``RPL_NAMREPLY``() =
        let clientData = ircClientData()
        let usersInChannel1 = [|"nick1"; "nick2"; "nick3"; "nick4"; "nick5"|]
        let usersInChannel2 = [|"nick6"; "nick7"; "nick8"; "nick9"; "nick10"|]

        let parameters = Some (toParameters ( usersInChannel1 |> Array.append [|"Nick"; "="; "#channel"|] ) )
        let verb = Some (Verb "RPL_NAMREPLY")
        let message = Message.NewSimpleMessage verb parameters
        rplNamReplyHandler (message, clientData)

        ``RPL_ENDOFNAMES`` (clientData, usersInChannel1)

        let parameters = Some (toParameters ( usersInChannel2 |> Array.append [|"Nick"; "="; "#channel"|] ) )
        let verb = Some (Verb "RPL_NAMREPLY")
        let message = Message.NewSimpleMessage verb parameters
        rplNamReplyHandler (message, clientData)

        ``RPL_ENDOFNAMES`` (clientData, usersInChannel1 |> Array.append usersInChannel2)

    [<Test>]
    let ``RPL_TOPIC``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "RPL_TOPIC")) (Some (toParameters [|"Nick"; "#channel"; "channel topic"|]))

        rplTopicHandler (message, clientData)

        let channelInfo = clientData.GetChannelInfo "#channel"
        Assert.True (channelInfo.IsSome, "channelInfo of clientData was None, it's supposed to be set to a value")
        let channelInfo = channelInfo.Value
        Assert.AreEqual (channelInfo.Topic, "channel topic")

    [<Test>]
    let ``RPL_AWAY internal handler should do nothing``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Client"; "Nick"; "I am away"|])
        let verb = Some (Verb "RPL_AWAY")
        let message = Message.NewSimpleMessage verb parameters

        Assert.Pass()
//#endregion Channel messages

//#region Error responses from server
    let verifyErrorResponse wantedResponse handler message (clientData: IRCClientData) =
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