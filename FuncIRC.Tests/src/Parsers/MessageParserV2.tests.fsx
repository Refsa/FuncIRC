#r "../../../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"

#load "../../../.paket/load/netstandard2.0/NUnit.fsx"
#load "../../../.paket/load/netstandard2.0/FParsec.fsx"
#load "../Utils/TestMessages.fsx"

namespace FuncIRC.Tests

open NUnit.Framework

module MessageParserTestsV2 =
    open FParsec
    open FParsec.CharParsers
    open FParsec.Primitives
    open FParsec.Error
    open FParsec.Internal
    open FParsec.Internals

    open FuncIRC
    open FuncIRC.MessageParserInternalsV2
    open FuncIRC.MessageParserV2
    open FuncIRC.MessageTypes
    open FuncIRC.Validators
    open FuncIRC.StringHelpers
    open TestMessages

    let fullTestMessageString = "@aaa=bbb;ccc;example.com/ddd=eee :nick!ident@host.com PRIVMSG me :Hello"
    let fullTestMessageString2 = "@aaa=bbb;ccc;example.com/ddd=eee :ident@host.com PRIVMSG me :Hello"
    let noTagsMessageString = ":irc.example.com CAP LS * :multi-prefix extended-join sasl"
    let onlyCommandMessageString = "CAP LS * :multi-prefix extended-join sasl"
    let nickOnlyMessageString = ":coolguy foo bar baz :asdf quux"

    let testMessage1 = "@tag1=value1;tag2;vendor1/tag3=value2;vendor2/tag4 COMMAND param1 param2 :param3 param3"
    let testMessage2 = "@a=b;c=32;k;rt=ql7 foo"

    let test p str =
        match run p str with
        | Success(result, _, _)   -> printfn "Success: %A" result; true
        | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg; false

    [<Test>]
    let testParserFunction() =
        test messageParser fullTestMessageString |> ignore
        test messageParser fullTestMessageString2 |> ignore
        test messageParser noTagsMessageString |> ignore
        test messageParser onlyCommandMessageString |> ignore
        test messageParser nickOnlyMessageString |> ignore

        test messageParser testMessage1 |> ignore
        test messageParser testMessage2 |> ignore

        //parseMessageString errorMessage3
        //|> fun m -> printfn "%A" m

        Assert.Fail()

    [<Test>]
    let testMessageParserV2() =
        let mutable threwExceptions = []

        testMessages
        |> List.iter 
            (fun x ->
                try
                    parseMessageString x.Input 
                    |> fun result -> Assert.AreEqual (x.Output, result, x.Input)
                with
                | e -> threwExceptions <- (x.Input, e.Message) :: threwExceptions
            )

        match threwExceptions.Length with
        | 0 -> Assert.True(true)
        | _ ->
            Assert.True(false, "Test OK, but Exception was thrown on messages:\n " + sprintf "%A" threwExceptions)