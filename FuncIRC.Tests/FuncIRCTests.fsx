#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"

namespace FuncIRC.Tests

open NUnit.Framework
open FuncIRC
open FuncIRC.MessageParser
open FuncIRC.MessageTypes

module MessageParserTest =
    let testIRCServerAddress = "testnet.inspircd.org"
    let testIRCServerChannel = "#refsa"
    let testIRCServerNick = "testbot"

    type TestMessage =
        { Input: string
          Output: Message }

    /// Some tests from: https://github.com/ircdocs/parser-tests/tree/master/tests
    /// Some messages from the official IRVv3 docs
    let testMessages =
        [   { Input = "@aaa=bbb;ccc;example.com/ddd=eee :nick!ident@host.com PRIVMSG me :Hello"
              Output =
                  { Tags = Some [{Key = "aaa"; Value = Some "bbb"}; {Key = "ccc"; Value = None}; {Key = "example.com/ddd"; Value = Some "eee"}] 
                    Source = Some {Nick = Some "nick"; User = Some "ident"; Host = Some "host.com" }
                    Verb = Some (Verb "PRIVMSG")
                    Params = Some (toParameters [ "me"; "Hello" ]) } }

            { Input = ":irc.example.com CAP LS * :multi-prefix extended-join sasl"
              Output =
                  { Tags = None
                    Source = Some {Nick = None; User = None; Host = Some "irc.example.com"}
                    Verb = Some (Verb "CAP")
                    Params = Some (toParameters [ "LS"; "*"; "multi-prefix extended-join sasl" ]) } }

            { Input = "@id=234AB :dan!d@localhost PRIVMSG #chan :Hey what's up!"
              Output =
                  { Tags = Some [ {Key = "id"; Value = Some "234AB"} ]
                    Source = Some {Nick = Some "dan"; User = Some "d"; Host = Some "localhost"}
                    Verb = Some (Verb "PRIVMSG")
                    Params = Some (toParameters [ "#chan"; "Hey what's up!" ]) } }

            { Input = "CAP REQ :sasl"
              Output =
                  { Tags = None
                    Source = None
                    Verb = Some (Verb "CAP")
                    Params = Some (toParameters [ "REQ"; "sasl" ]) } }

            { Input = ":irc.example.com CAP * LIST :"
              Output =
                  { Tags = None
                    Source = Some {Nick = None; User = None; Host = Some "irc.example.com"}
                    Verb = Some (Verb "CAP")
                    Params = Some (toParameters [ "*"; "LIST" ]) } }

            { Input = "CAP * LS :multi-prefix sasl"
              Output =
                  { Tags = None
                    Source = None
                    Verb = Some (Verb "CAP")
                    Params = Some (toParameters [ "*"; "LS"; "multi-prefix sasl" ]) } }

            { Input = "CAP REQ :sasl message-tags foo"
              Output =
                  { Tags = None
                    Source = None
                    Verb = Some (Verb "CAP")
                    Params = Some (toParameters [ "REQ"; "sasl message-tags foo" ]) } }

            { Input = ":dan!d@localhost PRIVMSG #chan :Hey"
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "dan"; User = Some "d"; Host = Some "localhost"}
                    Verb = Some (Verb "PRIVMSG")
                    Params = Some (toParameters [ "#chan"; "Hey" ]) } }

            { Input = ":dan!d@localhost PRIVMSG #chan Hey!"
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "dan"; User = Some "d"; Host = Some "localhost"}
                    Verb = Some (Verb "PRIVMSG")
                    Params = Some (toParameters [ "#chan"; "Hey!" ]) } }

            { Input = ""
              Output =
                  { Tags = None
                    Source = None
                    Verb = None
                    Params = None } }

            { Input = ":coolguy foo bar baz :asdf quux"
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "coolguy"; User = None; Host = None}
                    Verb = Some (Verb "foo")
                    Params = Some (toParameters [ "bar"; "baz"; "asdf quux" ]) } }

            { Input = ":coolguy PRIVMSG bar :lol :) "
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "coolguy"; User = None; Host = None}
                    Verb = Some (Verb "PRIVMSG")
                    Params = Some (toParameters [ "bar"; "lol :) " ]) } }

            { Input = ":gravel.mozilla.org 432  #momo :Erroneous Nickname: Illegal characters"
              Output =
                  { Tags = None
                    Source = Some {Nick = None; User = None; Host = Some "gravel.mozilla.org"}
                    Verb = Some (Verb "432")
                    Params = Some (toParameters [ "#momo"; "Erroneous Nickname: Illegal characters" ]) } }

            { Input = ":gravel.mozilla.org MODE #tckk +n "
              Output =
                  { Tags = None
                    Source = Some {Nick = None; User = None; Host = Some "gravel.mozilla.org"}
                    Verb = Some (Verb "MODE")
                    Params = Some (toParameters [ "#tckk"; "+n" ]) } }

            { Input = ":gravel.mozilla.org MODE :#tckk"
              Output =
                  { Tags = None
                    Source = Some {Nick = None; User = None; Host = Some "gravel.mozilla.org"}
                    Verb = Some (Verb "MODE")
                    Params = Some (toParameters [ "#tckk" ]) } }

            { Input = ":services.esper.net MODE #foo-bar +o foobar  "
              Output =
                  { Tags = None
                    Source = Some {Nick = None; User = None; Host = Some "services.esper.net"}
                    Verb = Some (Verb "MODE")
                    Params = Some (toParameters [ "#foo-bar"; "+o"; "foobar" ]) } }

            { Input = ":SomeOp MODE #channel :+i"
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "SomeOp"; User = None; Host = None}
                    Verb = Some (Verb "MODE")
                    Params = Some (toParameters [ "#channel"; "+i" ]) } }

            { Input = ":SomeOp MODE #channel +oo SomeUser :AnotherUser"
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "SomeOp"; User = None; Host = None}
                    Verb = Some (Verb "MODE")
                    Params = Some (toParameters [ "#channel"; "+oo"; "SomeUser"; "AnotherUser" ]) } }

            { Input = ":irc.example.com COMMAND param1 param2 :param3 param3"
              Output =
                  { Tags = None
                    Source = Some {Nick = None; User = None; Host = Some "irc.example.com"}
                    Verb = Some (Verb "COMMAND")
                    Params = Some (toParameters [ "param1"; "param2"; "param3 param3" ]) } }

            { Input = "@tag1=value1;tag2;vendor1/tag3=value2;vendor2/tag4 COMMAND param1 param2 :param3 param3"
              Output =
                  { Tags = Some [ {Key = "tag1"; Value = Some "value1"}; {Key = "tag2"; Value = None}; {Key = "vendor1/tag3"; Value = Some "value2"}; {Key = "vendor2/tag4"; Value = None} ] 
                    Source = None
                    Verb = Some (Verb "COMMAND")
                    Params = Some (toParameters [ "param1"; "param2"; "param3 param3" ]) } }

            { Input =
                  "@tag1=value1;tag2;vendor1/tag3=value2;vendor2/tag4= :irc.example.com COMMAND param1 param2 :param3 param3"
              Output =
                  { Tags = Some [ {Key = "tag1"; Value = Some "value1"}; {Key = "tag2"; Value = None}; {Key = "vendor1/tag3"; Value = Some "value2"}; {Key = "vendor2/tag4"; Value = None} ] 
                    Source = Some {Nick = None; User = None; Host = Some "irc.example.com"}
                    Verb = Some (Verb "COMMAND")
                    Params = Some (toParameters [ "param1"; "param2"; "param3 param3" ]) } }

            { Input = ":src AWAY"
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "src"; User = None; Host = None}
                    Verb = Some (Verb "AWAY")
                    Params = None } }

            { Input = ":src AWAY "
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "src"; User = None; Host = None}
                    Verb = Some (Verb "AWAY")
                    Params = None } }

            { Input = ":coolguy foo bar baz :  "
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "coolguy"; User = None; Host = None}
                    Verb = Some (Verb "foo")
                    Params = Some (toParameters [ "bar"; "baz"; "  " ]) } }

            { Input = "@a=b;c=32;k;rt=ql7 foo"
              Output =
                  { Tags = Some [ {Key = "a"; Value = Some "b"}; {Key = "c"; Value = Some "32"}; {Key = "k"; Value = None}; {Key = "rt"; Value = Some "ql7"} ] 
                    Source = None
                    Verb = Some (Verb "foo")
                    Params = None } }

            { Input = ":coolguy foo bar baz :  asdf quux "
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "coolguy"; User = None; Host = None}
                    Verb = Some (Verb "foo")
                    Params = Some (toParameters [ "bar"; "baz"; "  asdf quux " ]) } }

            { Input = "foo bar baz ::asdf"
              Output =
                  { Tags = None
                    Source = None
                    Verb = Some (Verb "foo")
                    Params = Some (toParameters [ "bar"; "baz"; ":asdf" ]) } }

            { Input = ":coolguy@127.0.0.1 foo bar baz ::asdf"
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "coolguy"; User = None; Host = Some "127.0.0.1"}
                    Verb = Some (Verb "foo")
                    Params = Some (toParameters [ "bar"; "baz"; ":asdf" ]) } }

            { Input = @":coolguy!ag@net\x035w\x03ork.admin PRIVMSG foo :bar baz"
              Output =
                  { Tags = None
                    Source = Some {Nick = Some "coolguy"; User = Some "ag"; Host = Some @"net\x035w\x03ork.admin"}
                    Verb = Some (Verb "PRIVMSG")
                    Params = Some (toParameters [ "foo"; "bar baz" ]) } }
        ]

    /// Prints contents of a Source type
    let printSource (source: Source) =
        printf "[ "
        if source.Nick.IsSome then
            printf "Nick = %s; " source.Nick.Value
        if source.User.IsSome then
            printf "User = %s; " source.User.Value
        if source.Host.IsSome then
            printf "Host = %s; " source.Host.Value                
        printf " ]"

    /// Expanded equality comparison between two Message Records
    let messageEquals msg1 msg2 =
        match (msg1, msg2) with
        | _ when msg1.Tags   <> msg2.Tags   -> printfn "Tags not equal";   false
        | _ when msg1.Source <> msg2.Source -> printfn "Source not equal"; false
        | _ when msg1.Verb   <> msg2.Verb   -> printfn "Verb not equal";   false
        | _ when msg1.Params <> msg2.Params -> printfn "Params not equal"; false
        | _ -> true

    /// Function to print out content of a message to console
    let printMessage message =
        printf "\tTags: "
        if message.Tags.IsSome then 
            printf "%A" message.Tags.Value
        printf "\n\tSource: "
        if message.Source.IsSome then
            printSource message.Source.Value
        printf "\n\tVerb: "
        if message.Verb.IsSome then 
            printf "%s" (string message.Verb.Value)
        printf "\n\tParams: "
        if message.Params.IsSome then 
            printf "%A" message.Params.Value
        printfn ""

    /// Verifies that a message parsed with messageSplit is correct
    let verifyMessageParser (messageString: string, wantedResult: Message) =
        let parsedMessage: Message = parseMessageString messageString

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
        testMessages 
        |> List.iter 
            (fun tm -> 
                Assert.True(verifyMessageParser (tm.Input, tm.Output)
            ))