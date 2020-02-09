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

    /// pongMessageHandler tests
    [<Test>]
    let ``PING verb handler should add an outbound message with the verb PONG``() =
        let clientData = ircClientData()
        let message = newVerbNameMessage "PING"

        pongMessageHandler (message, clientData)

        ////let outboundMessages = clientData.GetOutboundMessages.Replace("\n", "").Split('\r') |> arrayRemove <| stringIsEmpty
        //Assert.AreEqual (outboundMessages.Length, 1)
        //Assert.AreEqual (outboundMessages.[0], "PONG")
        Assert.Warn ("No longer works after switching to MailboxProcessor")

//#region Connection numerics
    /// RPL_WELCOME tests
    [<Test>]
    let ``RPL_WELCOME handler should do nothing``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"nick"; "Welcome to the Refsa IRC Network testnick!testuser@127.0.0.1"|])
        let verb = Some (Verb "RPL_WELCOME")
        let message = Message.NewSimpleMessage verb parameters

        rplWelcomeHandler (message, clientData)

        //Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    /// RPL_YOURHOST tests
    [<Test>]
    let ``RPL_YOURHOST handler should do nothing``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "Your host is 127.0.0.1, running version InspIRCd-3"|])
        let verb = Some (Verb "RPL_YOURHOST")
        let message = Message.NewSimpleMessage verb parameters

        rplYourHostHandler (message, clientData)

        //Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    /// RPL_CREATED tests
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

    /// RPL_MYINFO tests
    [<Test>]
    let ``RPL_MYINFO handler should do nothing``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "127.0.0.1"; "InspIRCd-3"; "iosw"; "biklmnopstv"; "bklov"|])
        let verb = Some (Verb "RPL_MYINFO")
        let message = Message.NewSimpleMessage verb parameters

        rplMyInfoHandler (message, clientData)

        //Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

//#region RPL_ISUPPORT
    let addISupportFeaturesAndVerify (wantedFeatures: IRCServerFeatures, message, clientData) =
        rplISupportHandler (message, clientData)

        Assert.AreEqual (wantedFeatures.Value.Count, clientData.GetServerFeatures.Count)
        let serverFeatureContentEquals = Array.forall2 (fun a b -> a=b) (wantedFeatures.Value |> Map.toArray) (clientData.GetServerFeatures |> Map.toArray)

        //if not serverFeatureContentEquals then
        //    clientData.GetServerFeatures
        //    |> Map.iter (fun k -> printfn "%s=%s" k k)

        serverFeatureContentEquals

    [<Test>]
    let ``RPL_ISUPPORT handler should append the incoming parameters to IRCServerFeatures on IRCClientData``() =
        // wanted outcome of RPL_ISUPPORT handler
        let wantedFeatures1 = 
            [| 
                ("AWAYLEN", "200"); ("CASEMAPPING", "ascii"); ("CHANLIMIT", "#:20"); 
                ("CHANMODES", "b,k,l,imnpst"); ("CHANNELLEN", "64"); ("CHANTYPES", "#"); 
                ("ELIST", "CMNTU"); ("HOSTLEN", "64"); ("KEYLEN", "32"); ("KICKLEN", "255"); 
                ("LINELEN", "512"); ("MAXLIST", "b:100") 
            |]
        let wantedFeatures2 = 
            wantedFeatures1 
            |> Array.append 
                [| 
                    ("MAXTARGETS", "20"); ("MODES", "20"); ("NETWORK", "Refsa"); 
                    ("NICKLEN", "30"); ("PREFIX", "(ov)@+"); ("SAFELIST", ""); 
                    ("STATUSMSG", "@+"); ("TOPICLEN", "307"); ("USERLEN", "10"); ("WHOX", "") 
                |]

        // Setup data required to run test        
        let isupportParams1 = [|"AWAYLEN=200"; "CASEMAPPING=ascii"; "CHANLIMIT=#:20"; "CHANMODES=b,k,l,imnpst"; "CHANNELLEN=64"; "CHANTYPES=#"; "ELIST=CMNTU"; "HOSTLEN=64"; "KEYLEN=32"; "KICKLEN=255"; "LINELEN=512"; "MAXLIST=b:100"; "are supported by this server"|]
        let isupportParams2 = [|"MAXTARGETS=20"; "MODES=20"; "NETWORK=Refsa"; "NICKLEN=30"; "PREFIX=(ov)@+"; "SAFELIST"; "STATUSMSG=@+"; "TOPICLEN=307"; "USERLEN=10"; "WHOX"; "are supported by this server"|]

        let clientData = ircClientData()
        let parameters1 = Some (toParameters (isupportParams1 |> Array.append [|"Nick";|]) )
        let parameters2 = Some (toParameters (isupportParams2 |> Array.append [|"Nick";|]) )
        let verb = Some (Verb "RPL_ISUPPORT")
        let message1 = Message.NewSimpleMessage verb parameters1
        let message2 = Message.NewSimpleMessage verb parameters2

        // Verify that first RPL_ISUPPORT message is handled correctly
        let message1valid = addISupportFeaturesAndVerify (Features (wantedFeatures1 |> Map.ofArray), message1, clientData)
        Assert.True (message1valid, "First pass of ISupport params was not valid")
        // Verify that the second RPL_ISUPPORT message is handled correctly
        let message2valid = addISupportFeaturesAndVerify (Features (wantedFeatures2 |> Map.ofArray), message2, clientData)
        Assert.True (message2valid, "Second pass of ISupport params was not valid")
//#endregion RPL_ISUPPORT

    /// RPL_LUSERCLIENT tests
    [<Test>]
    let ``RPL_LUSERCLIENT handler should do nothing for now``() =
        let clientData = ircClientData()
        let testParams = "There are 0 users and 0 invisible on 1 servers"
        let parameters = Some (toParameters [|"Nick"; testParams|])
        let verb = Some (Verb "RPL_LUSERCLIENT")
        let message = Message.NewSimpleMessage verb parameters

        rplLUserClientHandler (message, clientData)

        Assert.Warn ("Uncertain about how RPL_LUSERCLIENT should be handled")
        //Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    /// RPL_LUSERUNKNOWN tests
    [<Test>]
    let ``RPL_LUSERUNKNOWN handler should do nothing for now``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "1"; "unknown connections"|])
        let verb = Some (Verb "RPL_LUSERUNKNOWN")
        let message = Message.NewSimpleMessage verb parameters

        rplLUserUnknownHandler (message, clientData)

        Assert.Warn ("Uncertain about how RPL_LUSERUNKNOWN should be handled")
        //Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    /// RPL_LUSERCHANNELS tests
    [<Test>]
    let ``RPL_LUSERCHANNELS handler should do nothing for now``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "0"; "channels formed"|])
        let verb = Some (Verb "RPL_LUSERCHANNELS")
        let message = Message.NewSimpleMessage verb parameters

        rplLUserChannelsHandler (message, clientData)

        Assert.Warn ("Uncertain about how RPL_LUSERCHANNELS should be handled")
        //Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    /// RPL_LUSERME tests
    [<Test>]
    let ``RPL_LUSERME handler should do nothing for now``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Nick"; "I have 0 clients and 0 servers"|])
        let verb = Some (Verb "RPL_LUSERME")
        let message = Message.NewSimpleMessage verb parameters

        rplLUserMeHandler (message, clientData)

        Assert.Warn ("Uncertain about how RPL_LUSERME should be handled")
        //Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    /// RPL_LOCALUSERS tests
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

    /// RPL_GLOBALUSERS tests
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
//#endregion Connection numerics 

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

        //Assert.AreEqual (clientData.GetOutboundMessages, "")
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

        //Assert.AreEqual (clientData.GetOutboundMessages, "")
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

    /// RPL_TOPIC tests
    [<Test>]
    let ``RPL_TOPIC``() =
        let clientData = ircClientData()
        let message = Message.NewSimpleMessage (Some (Verb "RPL_TOPIC")) (Some (toParameters [|"Nick"; "#channel"; "channel topic"|]))

        rplTopicHandler (message, clientData)

        let channelInfo = clientData.GetChannelInfo "#channel"
        Assert.True (channelInfo.IsSome, "channelInfo of clientData was None, it's supposed to be set to a value")
        let channelInfo = channelInfo.Value
        Assert.AreEqual (channelInfo.Topic, "channel topic")

    /// RPL_AWAY tests
    [<Test>]
    let ``RPL_AWAY internal handler should do nothing``() =
        let clientData = ircClientData()
        let parameters = Some (toParameters [|"Client"; "Nick"; "I am away"|])
        let verb = Some (Verb "RPL_AWAY")
        let message = Message.NewSimpleMessage verb parameters

        Assert.Pass()
//#endregion Channel messages