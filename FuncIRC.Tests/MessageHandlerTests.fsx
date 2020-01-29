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

        Assert.Warn ("Not Implemented")

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

        Assert.AreEqual (clientData.GetServerMOTD.Length, motdContents.Length)
        Assert.True (List.forall2 (fun a b -> a=b) clientData.GetServerMOTD motdContents)

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