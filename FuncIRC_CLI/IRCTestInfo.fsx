let testIRCServerAddress = "testnet.inspircd.org"
let testIRCServerChannel = "#refsa"
let testIRCServerNick = "testbot"

let testMessage0 = "@aaa=bbb;ccc;example.com/ddd=eee :nick!ident@host.com PRIVMSG me :Hello"
let testMessage1 = ":irc.example.com CAP LS * :multi-prefix extended-join sasl"
let testMessage2 = "@id=234AB :dan!d@localhost PRIVMSG #chan :Hey what's up!"
let testMessage3 = "CAP REQ :sasl"
let testMessage4 = ":irc.example.com CAP * LIST :"       // ->  ["*", "LIST", ""]
let testMessage5 = "CAP * LS :multi-prefix sasl"         // ->  ["*", "LS", "multi-prefix sasl"]
let testMessage6 = "CAP REQ :sasl message-tags foo"      // ->  ["REQ", "sasl message-tags foo"]
let testMessage7 = ":dan!d@localhost PRIVMSG #chan :Hey" // ->  ["#chan", "Hey!"]
let testMessage8 = ":dan!d@localhost PRIVMSG #chan Hey!" // ->  ["#chan", "Hey!"]