#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests
namespace NUnit
namespace NUnit.Framework

module MessageHandlerTests =
    open FuncIRC.IRCClientData
    open FuncIRC.MessageTypes
    open FuncIRC.MessageHandlers
    open FuncIRC.GeneralHelpers
    open NUnit.Framework

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

        Assert.AreEqual (clientData.GetOutboundMessages, "")
        Assert.AreEqual (clientData.GetUserInfoSelf, None)

    [<Test>]
    let ``RPL_CREATED handler should respond with trailing params if there was any``() =
        let testParams = "This server was created 23:25:21 Jan 24 2020"
        let parameters = Some (toParameters [|"Nick"; testParams|])
        let verb = Verb "RPL_CREATED"
        Assert.Warn ("Not Implemented")

    [<Test>]
    let ``RPL_MYINFO handler should respond with params if there was any``() =
        let testParams = toParameters [|"Nick"; "127.0.0.1 InspIRCd-3 iosw biklmnopstv"; "bklov"|]
        let verb = Verb "RPL_MYINFO"
        Assert.Warn ("Not Implemented")

    [<Test>]
    let ``RPL_ISUPPORT handler should respond with everything except trailing params``() =
        let testParams = toParameters [|"Nick"; "AWAYLEN=200"; "CASEMAPPING=ascii"; "CHANLIMIT=#:20"; "CHANMODES=b,k,l,imnpst"; "CHANNELLEN=64"; "CHANTYPES=#"; "ELIST=CMNTU"; "HOSTLEN=64"; "KEYLEN=32"; "KICKLEN=255"; "LINELEN=512"; "MAXLIST=b:100"; "are supported by this server"|]
        let verb = Verb "RPL_ISUPPORT"
        Assert.Warn ("Not Implemented")

    [<Test>]
    let ``RPL_LUSERCLIENT handler should respond with trailing params``() =
        let testParams = "There are 0 users and 0 invisible on 1 servers"
        let parameters = Some (toParameters [|"Nick"; testParams|])
        let verb = Verb "RPL_LUSERCLIENT"
        Assert.Warn ("Not Implemented")

    [<Test>]
    let ``RPL_LUSERUNKNOWN handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "1"; "unknown connections"|]
        let verb = Verb "RPL_LUSERUNKNOWN"
        Assert.Warn ("Not Implemented")

    [<Test>]
    let ``RPL_LUSERCHANNELS handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "0"; "channels formed"|]
        let verb = Verb "RPL_LUSERCHANNELS"
        Assert.Warn ("Not Implemented")

    [<Test>]
    let ``RPL_LUSERME handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "I have 0 clients and 0 servers"|]
        let verb = Verb "RPL_LUSERME"
        Assert.Warn ("Not Implemented")

    [<Test>]
    let ``RPL_LOCALUSERS handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "Current local users: 0  Max: 0"|]
        let verb = Verb "RPL_LOCALUSERS"
        Assert.Warn ("Not Implemented")

    [<Test>]
    let ``RPL_GLOBALUSERS handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "Current global users: 0  Max: 0"|]
        let verb = Verb "RPL_GLOBALUSERS"
        Assert.Warn ("Not Implemented")

    [<Test>]
    let ``RPL_MOTDSTART handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "127.0.0.1 message of the day"|]
        let verb = Verb "RPL_MOTDSTART"
        Assert.Warn ("Not Implemented")

    [<Test>]
    let ``RPL_MOTD handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; " _____                        _____   _____    _____      _"|]
        let verb = Verb "RPL_MOTD"
        Assert.Warn ("Not Implemented")

    [<Test>]
    let ``RPL_ENDOFMOTD handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "End of message of the day."|]
        let verb = Verb "RPL_ENDOFMOTD"
        Assert.Warn ("Not Implemented")