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
    open FuncIRC.MessageTypes
    open FuncIRC.Validators
    open FuncIRC.StringHelpers
    open TestMessages

    let fullTestMessageString = "@aaa=bbb;ccc;example.com/ddd=eee :nick!ident@host.com PRIVMSG me :Hello"
    let fullTestMessageString2 = "@aaa=bbb;ccc;example.com/ddd=eee :ident@host.com PRIVMSG me :Hello"
    let noTagsMessageString = ":irc.example.com CAP LS * :multi-prefix extended-join sasl"
    let onlyCommandMessageString = "CAP LS * :multi-prefix extended-join sasl"

    let test p str =
        match run p str with
        | Success(result, _, _)   -> printfn "Success: %A" result; true
        | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg; false

    [<Test>]
    let testParserFunction() =
        //test tagsPart fullTestMessageString
        //test tagsParser fullTestMessageString
        //test tagsParser noTagsMessageString
        test messageParser fullTestMessageString |> ignore
        test messageParser fullTestMessageString2 |> ignore
        test messageParser noTagsMessageString |> ignore
        test messageParser onlyCommandMessageString |> ignore

        Assert.True(false)