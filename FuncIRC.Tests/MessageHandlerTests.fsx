#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests

module MessageHandlerTests =
    open FuncIRC.MessageTypes
    open NUnit.Framework

    [<Test>]
    let ``PING verb handler should respond with PONG``() =
        let verb = Verb "PING"
        Assert.True (false)

    [<Test>]
    let ``PRIVMSG verb handler should respond with last entry of parameters and Callback type``() =
        let parameters = Some (toParameters [|"#chan"; "some message to channel"|])
        let verb = Verb "PRIVMSG"
        Assert.True (false)

    [<Test>]
    let ``NOTICE verb handler should respond with NOTICE and Callback type``() =
        let verb = Verb "NOTICE"
        Assert.True (false)

    [<Test>]
    let ``RPL_WELCOME handler should respond with Welcome message and Callback type``() =
        let parameters = Some (toParameters [|"nick"; "Welcome to the Refsa IRC Network testnick!testuser@127.0.0.1"|])
        let verb = Verb "001"
        Assert.True (false)

    [<Test>]
    let ``RPL_YOURHOST handler should respond with trailing params if there was any``() =
        let parameters = Some (toParameters [|"Nick"; "Your host is 127.0.0.1, running version InspIRCd-3"|])
        let verb = Verb "002"
        Assert.True (false)

    [<Test>]
    let ``RPL_CREATED handler should respond with trailing params if there was any``() =
        let testParams = "This server was created 23:25:21 Jan 24 2020"
        let parameters = Some (toParameters [|"Nick"; testParams|])
        let verb = Verb "003"
        Assert.True (false)

    [<Test>]
    let ``RPL_MYINFO handler should respond with params if there was any``() =
        let testParams = toParameters [|"Nick"; "127.0.0.1 InspIRCd-3 iosw biklmnopstv"; "bklov"|]
        let verb = Verb "004"
        Assert.True (false)

    [<Test>]
    let ``RPL_ISUPPORT handler should respond with everything except trailing params``() =
        let testParams = toParameters [|"Nick"; "AWAYLEN=200"; "CASEMAPPING=ascii"; "CHANLIMIT=#:20"; "CHANMODES=b,k,l,imnpst"; "CHANNELLEN=64"; "CHANTYPES=#"; "ELIST=CMNTU"; "HOSTLEN=64"; "KEYLEN=32"; "KICKLEN=255"; "LINELEN=512"; "MAXLIST=b:100"; "are supported by this server"|]
        let verb = Verb "005"
        Assert.True (false)

    [<Test>]
    let ``RPL_LUSERCLIENT handler should respond with trailing params``() =
        let testParams = "There are 0 users and 0 invisible on 1 servers"
        let parameters = Some (toParameters [|"Nick"; testParams|])
        let verb = Verb "251"
        Assert.True (false)

    [<Test>]
    let ``RPL_LUSERUNKNOWN handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "1"; "unknown connections"|]
        let verb = Verb "253"
        Assert.True (false)

    [<Test>]
    let ``RPL_LUSERCHANNELS handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "0"; "channels formed"|]
        let verb = Verb "254"
        Assert.True (false)

    [<Test>]
    let ``RPL_LUSERME handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "I have 0 clients and 0 servers"|]
        let verb = Verb "255"
        Assert.True (false)

    [<Test>]
    let ``RPL_LOCALUSERS handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "Current local users: 0  Max: 0"|]
        let verb = Verb "265"
        Assert.True (false)

    [<Test>]
    let ``RPL_GLOBALUSERS handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "Current global users: 0  Max: 0"|]
        let verb = Verb "266"
        Assert.True (false)

    [<Test>]
    let ``RPL_MOTDSTART handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "127.0.0.1 message of the day"|]
        let verb = Verb "375"
        Assert.True (false)

    [<Test>]
    let ``RPL_MOTD handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; " _____                        _____   _____    _____      _"|]
        let verb = Verb "372"
        Assert.True (false)

    [<Test>]
    let ``RPL_ENDOFMOTD handler should respond with trailing params``() =
        let testParams = toParameters [|"Nick"; "End of message of the day."|]
        let verb = Verb "376"
        Assert.True (false)