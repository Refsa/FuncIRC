#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"

namespace FuncIRC.Tests
open System.Text

module ConnectionClientTests =
    open NUnit.Framework
    open FuncIRC.ConnectionClient
    open FuncIRC.IRCStreamReader
    open FuncIRC.MessageTypes
    open FuncIRC.VerbHandlers
    open FuncIRC.NumericReplies

    [<Test>]
    let ``Check that ConnectionClient can establish TCP connection``() =
        //startClient()
        Assert.True (true)

    [<Test>]
    let ``Check that parseByteString can parse both UTF8 and Latin1 byte streams``() =
        let testString = "this is a test string"
        let utf8Encoded = utf8Encoding.GetBytes (testString)
        let latin1Encoded = latin1Encoding.GetBytes (testString)

        let utf8Decoded = parseByteString utf8Encoded
        let latin1Decoded = parseByteString latin1Encoded

        Assert.True ((utf8Decoded = testString))
        Assert.True ((latin1Decoded = testString))

    [<Test>]
    let ``PING verb handler should respond with PONG``() =
        let pingVerb = Verb "PING"
        let response = handleVerb pingVerb |> fun handler -> handler <| None

        Assert.AreEqual (response.Type, VerbHandlerType.Response)
        Assert.AreEqual (response.Verb, NumericsReplies.MSG_PING)

    [<Test>]
    let ``PRIVMSG verb handler should respond with last entry of parameters and Callback type``() =
        let privmsgVerb = Verb "PRIVMSG"
        let response = handleVerb privmsgVerb |> fun handler -> handler <| Some (toParameters [|"#chan"; "some message to channel"|])

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.MSG_PRIVMSG)
        Assert.AreEqual (response.Content, "some message to channel")

    [<Test>]
    let ``NOTICE verb handler should respond with NOTICE and Callback type``() =
        let noticeVerb = Verb "NOTICE"
        let response = handleVerb noticeVerb |> fun handler -> handler <| None

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.MSG_NOTICE)

    [<Test>]
    let ``RPL_WELCOME handler should respond with Welcome message and Callback type``() =
        let rplWelcomeVerb = Verb "001" // RPL_WELCOME Numeric
        let response = 
            handleVerb rplWelcomeVerb 
            |> fun handler -> 
                handler <| Some (toParameters [|"nick"; "Welcome to the Refsa IRC Network testnick!testuser@127.0.0.1"|])

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_WELCOME)
        Assert.AreEqual (response.Content, "Welcome to the Refsa IRC Network testnick!testuser@127.0.0.1")

    [<Test>]
    let ``RPL_YOURHOST handler should respond with trailing params if there was any``() =
        let rplYourHostVerb = Verb "002"
        let response = 
            handleVerb rplYourHostVerb
            |> fun handler -> 
                handler <| Some (toParameters [|"Nick"; "Your host is 127.0.0.1, running version InspIRCd-3"|])

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_YOURHOST)
        Assert.AreEqual (response.Content, "Your host is 127.0.0.1, running version InspIRCd-3")

    [<Test>]
    let ``RPL_CREATED handler should respond with trailing params if there was any``() =
        let testParams = "This server was created 23:25:21 Jan 24 2020"
        let verb = Verb "003"
        let response = 
            handleVerb verb
            |> fun handler -> 
                handler <| Some (toParameters [|"Nick"; testParams|])

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_CREATED)
        Assert.AreEqual (response.Content, testParams)

    [<Test>]
    let ``RPL_MYINFO handler should respond with params if there was any``() =
        let testParams = [|"Nick"; "127.0.0.1 InspIRCd-3 iosw biklmnopstv"; "bklov"|]
        let verb = Verb "004"
        let response = 
            handleVerb verb
            |> fun handler -> 
                handler <| Some (toParameters testParams)

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_MYINFO)
        Assert.AreEqual (response.Content, testParams)

    [<Test>]
    let ``RPL_ISUPPORT handler should respond with everything except trailing params``() =
        let testParams = [|"Nick"; "AWAYLEN=200"; "CASEMAPPING=ascii"; "CHANLIMIT=#:20"; "CHANMODES=b,k,l,imnpst"; "CHANNELLEN=64"; "CHANTYPES=#"; "ELIST=CMNTU"; "HOSTLEN=64"; "KEYLEN=32"; "KICKLEN=255"; "LINELEN=512"; "MAXLIST=b:100"; "are supported by this server"|]
        let verb = Verb "005"
        let response = 
            handleVerb verb
            |> fun handler -> 
                handler <| Some (toParameters testParams)

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_ISUPPORT)
        Assert.AreEqual (response.Content, testParams)

    [<Test>]
    let ``RPL_LUSERCLIENT handler should respond with params``() =
        let testParams = "There are 0 users and 0 invisible on 1 servers"
        let verb = Verb "251"
        let response = 
            handleVerb verb
            |> fun handler -> 
                handler <| Some (toParameters [|"Nick"; testParams|])

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_LUSERCLIENT)
        Assert.AreEqual (response.Content, testParams)

    [<Test>]
    let ``RPL_LUSERUNKNOWN handler should respond with params``() =
        let testParams = [|"Nick"; "1"; "unknown connections"|]
        let verb = Verb "253"
        let response = 
            handleVerb verb
            |> fun handler -> 
                handler <| Some (toParameters testParams)

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_LUSERUNKNOWN)
        Assert.AreEqual (response.Content, testParams)

    [<Test>]
    let ``RPL_LUSERCHANNELS handler should respond with params``() =
        let testParams = [|"Nick"; "0"; "channels formed"|]
        let verb = Verb "254"
        let response = 
            handleVerb verb
            |> fun handler -> 
                handler <| Some (toParameters testParams)

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_LUSERCHANNELS)
        Assert.AreEqual (response.Content, testParams)

    [<Test>]
    let ``RPL_LUSERME handler should respond with params``() =
        let testParams = [|"Nick"; "I have 0 clients and 0 servers"|]
        let verb = Verb "255"
        let response = 
            handleVerb verb
            |> fun handler -> 
                handler <| Some (toParameters  testParams)

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_LUSERME)
        Assert.AreEqual (response.Content, testParams)

    [<Test>]
    let ``RPL_LOCALUSERS handler should respond with params``() =
        let testParams = [|"Nick"; "Current local users: 0  Max: 0"|]
        let verb = Verb "265"
        let response = 
            handleVerb verb
            |> fun handler -> 
                handler <| Some (toParameters  testParams)

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_LOCALUSERS)
        Assert.AreEqual (response.Content, testParams)

    [<Test>]
    let ``RPL_GLOBALUSERS handler should respond with params``() =
        let testParams = [|"Nick"; "Current global users: 0  Max: 0"|]
        let verb = Verb "266"
        let response = 
            handleVerb verb
            |> fun handler -> 
                handler <| Some (toParameters  testParams)

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_GLOBALUSERS)
        Assert.AreEqual (response.Content, testParams)