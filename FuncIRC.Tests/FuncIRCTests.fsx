#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"

namespace FuncIRC.Tests

open NUnit.Framework
open FuncIRC
open FuncIRC.MessageParser

module MessageParserTest =
    let testIRCServerAddress = "testnet.inspircd.org"
    let testIRCServerChannel = "#refsa"
    let testIRCServerNick = "testbot"

    type TestMessage = 
        {
            Input: string
            Output: Message
        }

    let testMessages =
        [
            {Input = "@aaa=bbb;ccc;example.com/ddd=eee :nick!ident@host.com PRIVMSG me :Hello"; 
             Output = {Tags = Some ["aaa=bbb"; "ccc"; "example.com/ddd=eee"]; Source = Some "nick!ident@host.com"; Verb = Some "PRIVMSG me :Hello"; Params = None}};
             
            {Input = ":irc.example.com CAP LS * :multi-prefix extended-join sasl"; 
             Output = {Tags = None; Source = Some "irc.example.com"; Verb = Some "CAP LS * :multi-prefix extended-join sasl"; Params = None}};
             
            {Input = "@id=234AB :dan!d@localhost PRIVMSG #chan :Hey what's up!"; 
             Output = {Tags = Some ["id=234AB"]; Source = Some "dan!d@localhost"; Verb = Some "PRIVMSG #chan :Hey what's up!"; Params = None}};
             
            {Input = "CAP REQ :sasl"; 
             Output = {Tags = None; Source = None; Verb = Some "CAP REQ :sasl"; Params = None}};
             
            {Input = ":irc.example.com CAP * LIST :"; 
             Output = {Tags = None; Source = Some "irc.example.com"; Verb = Some "CAP * LIST :"; Params = None}};
             
            {Input = "CAP * LS :multi-prefix sasl"; 
             Output = {Tags = None; Source = None; Verb = Some "CAP * LS :multi-prefix sasl"; Params = None}};
             
            {Input = "CAP REQ :sasl message-tags foo"; 
             Output = {Tags = None; Source = None; Verb = Some "CAP REQ :sasl message-tags foo"; Params = None}};
             
            {Input = ":dan!d@localhost PRIVMSG #chan :Hey" ; 
             Output = {Tags = None; Source = Some "dan!d@localhost"; Verb = Some "PRIVMSG #chan :Hey"; Params = None}};

            {Input = ":dan!d@localhost PRIVMSG #chan Hey!"; 
             Output = {Tags = None; Source = Some "dan!d@localhost"; Verb = Some "PRIVMSG #chan Hey!"; Params = None}};
        ]

    let verifyMessageParser (messageString: string, wantedResult: Message) =
        let parsedMessage: Message = messageSplit messageString
        if parsedMessage = wantedResult then true
        else 
            printfn "\nError on: %s" messageString

            printfn "Parsed Result:"
            parsedMessage.Print

            printfn "Wanted Result:"
            wantedResult.Print
            false

    [<Test>]
    let ``message parser should extract tags source verb and params``() =
        testMessages
        |> List.iter (fun tm -> Assert.True(verifyMessageParser (tm.Input, tm.Output)))