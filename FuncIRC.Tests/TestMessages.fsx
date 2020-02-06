#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"

namespace FuncIRC.Tests

module TestMessages = 
    open FuncIRC.MessageTypes

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

    /// Used to test hostname validator
    type HostnameTest = { Hostname: string; Valid: bool }

    /// Test cases for hostname validator
    let hostnameTests =
        [
            // True Tests
            {Hostname = "irc.example.com";  Valid = true}
            {Hostname = "i.coolguy.net";    Valid = true}
            {Hostname = "irc-srv.net.uk";   Valid = true}
            {Hostname = "iRC.CooLguY.NeT";  Valid = true}
            {Hostname = "gsf.ds342.co.uk";  Valid = true}
            {Hostname = "324.net.uk";       Valid = true}
            {Hostname = "xn--bcher-kva.ch"; Valid = true}
            {Hostname = "Xn--bcher-kva.ch"; Valid = true}
            // False Tests
            {Hostname = "-lol-.net.uk";          Valid = false}
            {Hostname = "-lol.net.uk";           Valid = false}
            {Hostname = "_irc._sctp.lol.net.uk"; Valid = false}
            {Hostname = "irc";                   Valid = false}
            {Hostname = "com";                   Valid = false}
            {Hostname = "";                      Valid = false}
        ]



    type SourceTest = {Source: Source; Valid: bool}

    let sourceTests =
        [
            {Source = {Host = Some "irc.example.com"; User = Some "user";  Nick = Some "nick"};  Valid = true}
            {Source = {Host = Some "irc.example.com"; User = Some "user1"; Nick = Some "nick1"}; Valid = true}
            {Source = {Host = Some "irc.example.com"; User = Some "user2"; Nick = Some "nick2"}; Valid = true}
            {Source = {Host = Some "irc.example.com"; User = Some "user3"; Nick = Some "nick3"}; Valid = true}

            {Source = {Host = Some "-lol-.net.uk"; User = Some "user";  Nick = Some "nick"};  Valid = false}
            {Source = {Host = Some "-lol-.net.uk"; User = Some "user1"; Nick = Some "nick1"}; Valid = false}
            {Source = {Host = Some "-lol-.net.uk"; User = Some "user2"; Nick = Some "nick2"}; Valid = false}
            {Source = {Host = Some "-lol-.net.uk"; User = Some "user3"; Nick = Some "nick3"}; Valid = false}
            {Source = {Host = Some "-lol-.net.uk"; User = Some "user3"; Nick = Some ""};      Valid = false}
            {Source = {Host = Some "-lol-.net.uk"; User = Some "";      Nick = Some "nick3"}; Valid = false}
        ]