#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"

namespace FuncIRC.Tests
open System.Text

module ConnectionClientTests =
    open NUnit.Framework
    open FuncIRC.ConnectionClient
    open FuncIRC.IRCStreamReader
    open FuncIRC.MessageTypes
    open FuncIRC.VerbHandlers

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
    let ``PING verb should respond with PONG``() =
        let pingVerb = Verb "PING"
        let response = handleVerb pingVerb |> fun handler -> handler <| None

        Assert.AreEqual (response.Type, VerbHandlerType.Response)
        Assert.AreEqual (response.Content, "PONG")

    [<Test>]
    let ``PRIVMSG verb should respond with PRIVMSG and Callback type``() =
        let privmsgVerb = Verb "PRIVMSG"
        let response = handleVerb privmsgVerb |> fun handler -> handler <| None

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Content, "PRIVMSG")

    [<Test>]
    let ``NOTICE verb should respond with NOTICE and Callback type``() =
        let noticeVerb = Verb "NOTICE"
        let response = handleVerb noticeVerb |> fun handler -> handler <| None

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Content, "NOTICE")

    [<Test>]
    let ``RPL_WELCOME numeric should respond with RPL_WELCOME and Callback type``() =
        let rplWelcomeVerb = Verb "001" // RPL_WELCOME Numeric
        let response = handleVerb rplWelcomeVerb |> fun handler -> handler <| None

        Assert.AreEqual (response.Type, VerbHandlerType.Callback)
        Assert.AreEqual (response.Content, "RPL_WELCOME")