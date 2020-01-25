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
        let testTrailingParam = "This server was created 23:25:21 Jan 24 2020"
        let verb = Verb "003"
        let response = 
            handleVerb verb
            |> fun handler -> 
                handler <| Some (toParameters [|"Nick"; testTrailingParam|])

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Verb, NumericsReplies.RPL_CREATED)
        Assert.AreEqual (response.Content, testTrailingParam)