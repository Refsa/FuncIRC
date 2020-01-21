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
        { Input: string
          Output: Message }

    let testMessages =
        [ { Input = "@aaa=bbb;ccc;example.com/ddd=eee :nick!ident@host.com PRIVMSG me :Hello"
            Output =
                { Tags = Some [{Key = "aaa"; Value = "bbb"}; {Key = "ccc"; Value = ""}; {Key = "example.com/ddd"; Value = "eee"}] 
                  Source = Some "nick!ident@host.com"
                  Verb = Some "PRIVMSG"
                  Params = Some [ "me"; "Hello" ] } }

          { Input = ":irc.example.com CAP LS * :multi-prefix extended-join sasl"
            Output =
                { Tags = None
                  Source = Some "irc.example.com"
                  Verb = Some "CAP"
                  Params = Some [ "LS"; "*"; "multi-prefix extended-join sasl" ] } }

          { Input = "@id=234AB :dan!d@localhost PRIVMSG #chan :Hey what's up!"
            Output =
                { Tags = Some [ {Key = "id"; Value = "234AB"} ]
                  Source = Some "dan!d@localhost"
                  Verb = Some "PRIVMSG"
                  Params = Some [ "#chan"; "Hey what's up!" ] } }

          { Input = "CAP REQ :sasl"
            Output =
                { Tags = None
                  Source = None
                  Verb = Some "CAP"
                  Params = Some [ "REQ"; "sasl" ] } }

          { Input = ":irc.example.com CAP * LIST :"
            Output =
                { Tags = None
                  Source = Some "irc.example.com"
                  Verb = Some "CAP"
                  Params = Some [ "*"; "LIST" ] } }

          { Input = "CAP * LS :multi-prefix sasl"
            Output =
                { Tags = None
                  Source = None
                  Verb = Some "CAP"
                  Params = Some [ "*"; "LS"; "multi-prefix sasl" ] } }

          { Input = "CAP REQ :sasl message-tags foo"
            Output =
                { Tags = None
                  Source = None
                  Verb = Some "CAP"
                  Params = Some [ "REQ"; "sasl message-tags foo" ] } }

          { Input = ":dan!d@localhost PRIVMSG #chan :Hey"
            Output =
                { Tags = None
                  Source = Some "dan!d@localhost"
                  Verb = Some "PRIVMSG"
                  Params = Some [ "#chan"; "Hey" ] } }

          { Input = ":dan!d@localhost PRIVMSG #chan Hey!"
            Output =
                { Tags = None
                  Source = Some "dan!d@localhost"
                  Verb = Some "PRIVMSG"
                  Params = Some [ "#chan"; "Hey!" ] } }

          { Input = ""
            Output =
                { Tags = None
                  Source = None
                  Verb = None
                  Params = None } }

          { Input = ":coolguy foo bar baz :asdf quux"
            Output =
                { Tags = None
                  Source = Some "coolguy"
                  Verb = Some "foo"
                  Params = Some [ "bar"; "baz"; "asdf quux" ] } }

          { Input = ":coolguy PRIVMSG bar :lol :) "
            Output =
                { Tags = None
                  Source = Some "coolguy"
                  Verb = Some "PRIVMSG"
                  Params = Some [ "bar"; "lol :) " ] } }

          { Input = ":gravel.mozilla.org 432  #momo :Erroneous Nickname: Illegal characters"
            Output =
                { Tags = None
                  Source = Some "gravel.mozilla.org"
                  Verb = Some "432"
                  Params = Some [ "#momo"; "Erroneous Nickname: Illegal characters" ] } }

          { Input = ":gravel.mozilla.org MODE #tckk +n "
            Output =
                { Tags = None
                  Source = Some "gravel.mozilla.org"
                  Verb = Some "MODE"
                  Params = Some [ "#tckk"; "+n" ] } }

          { Input = ":gravel.mozilla.org MODE :#tckk"
            Output =
                { Tags = None
                  Source = Some "gravel.mozilla.org"
                  Verb = Some "MODE"
                  Params = Some [ "#tckk" ] } }

          { Input = ":services.esper.net MODE #foo-bar +o foobar  "
            Output =
                { Tags = None
                  Source = Some "services.esper.net"
                  Verb = Some "MODE"
                  Params = Some [ "#foo-bar"; "+o"; "foobar" ] } }

          { Input = ":SomeOp MODE #channel :+i"
            Output =
                { Tags = None
                  Source = Some "SomeOp"
                  Verb = Some "MODE"
                  Params = Some [ "#channel"; "+i" ] } }

          { Input = ":SomeOp MODE #channel +oo SomeUser :AnotherUser"
            Output =
                { Tags = None
                  Source = Some "SomeOp"
                  Verb = Some "MODE"
                  Params = Some [ "#channel"; "+oo"; "SomeUser"; "AnotherUser" ] } }

          { Input = ":irc.example.com COMMAND param1 param2 :param3 param3"
            Output =
                { Tags = None
                  Source = Some "irc.example.com"
                  Verb = Some "COMMAND"
                  Params = Some [ "param1"; "param2"; "param3 param3" ] } }

          { Input = "@tag1=value1;tag2;vendor1/tag3=value2;vendor2/tag4 COMMAND param1 param2 :param3 param3"
            Output =
                { Tags = Some [ {Key = "tag1"; Value = "value1"}; {Key = "tag2"; Value = ""}; {Key = "vendor1/tag3"; Value = "value2"}; {Key = "vendor2/tag4"; Value = ""} ] 
                  Source = None
                  Verb = Some "COMMAND"
                  Params = Some [ "param1"; "param2"; "param3 param3" ] } }

          { Input =
                "@tag1=value1;tag2;vendor1/tag3=value2;vendor2/tag4= :irc.example.com COMMAND param1 param2 :param3 param3"
            Output =
                { Tags = Some [ {Key = "tag1"; Value = "value1"}; {Key = "tag2"; Value = ""}; {Key = "vendor1/tag3"; Value = "value2"}; {Key = "vendor2/tag4"; Value = ""} ] 
                  Source = Some "irc.example.com"
                  Verb = Some "COMMAND"
                  Params = Some [ "param1"; "param2"; "param3 param3" ] } }

          { Input = ":src AWAY"
            Output =
                { Tags = None
                  Source = Some "src"
                  Verb = Some "AWAY"
                  Params = None } }

          { Input = ":src AWAY "
            Output =
                { Tags = None
                  Source = Some "src"
                  Verb = Some "AWAY"
                  Params = None } }

          { Input = ":coolguy foo bar baz :  "
            Output =
                { Tags = None
                  Source = Some "coolguy"
                  Verb = Some "foo"
                  Params = Some [ "bar"; "baz"; "  " ] } }

          { Input = "@a=b;c=32;k;rt=ql7 foo"
            Output =
                { Tags = Some [ {Key = "a"; Value = "b"}; {Key = "c"; Value = "32"}; {Key = "k"; Value = ""}; {Key = "rt"; Value = "ql7"} ] 
                  Source = None
                  Verb = Some "foo"
                  Params = None } }

          { Input = ":coolguy foo bar baz :  asdf quux "
            Output =
                { Tags = None
                  Source = Some "coolguy"
                  Verb = Some "foo"
                  Params = Some [ "bar"; "baz"; "  asdf quux " ] } }

          { Input = "foo bar baz ::asdf"
            Output =
                { Tags = None
                  Source = None
                  Verb = Some "foo"
                  Params = Some [ "bar"; "baz"; ":asdf" ] } }

          { Input = @":coolguy!ag@net\x035w\x03ork.admin PRIVMSG foo :bar baz"
            Output =
                { Tags = None
                  Source = Some @"coolguy!ag@net\x035w\x03ork.admin"
                  Verb = Some "PRIVMSG"
                  Params = Some [ "foo"; "bar baz" ] } } ]

    /// Function to print out content of a message to console
    let printMessage message =
        printf "\tTags: "
        if message.Tags.IsSome then printf "%A" message.Tags.Value
        printf "\n\tSource: "
        if message.Source.IsSome then printf "%s" message.Source.Value
        printf "\n\tVerb: "
        if message.Verb.IsSome then printf "%s" message.Verb.Value
        printf "\n\tParams: "
        if message.Params.IsSome then printf "%A" message.Params.Value
        printfn ""

    /// Verifies that a message parsed with messageSplit is correct
    let verifyMessageParser (messageString: string, wantedResult: Message) =
        let parsedMessage: Message = messageSplit messageString

        if (messageEquals parsedMessage wantedResult) then
            true
        else
            printfn "\nError on: %s" messageString

            printfn "Parsed Result:"
            printMessage parsedMessage

            printfn "Wanted Result:"
            printMessage wantedResult
            false

    [<Test>]
    let ``message parser should extract tags source verb and params``() =
        testMessages |> List.iter (fun tm -> Assert.True(verifyMessageParser (tm.Input, tm.Output)))