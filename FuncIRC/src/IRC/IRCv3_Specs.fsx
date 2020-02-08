/// https://ircv3.net/specs/extensions/message-tags
module Specs =
    // Based on IRCv3 Client Specs from https://modern.ircdocs.horse/

    (* 
     ANBF reperesentation of an IRC message
        message     =  [ "@" tags SPACE ] [ ":" prefix SPACE ] command
                       [ params ] crlf

        tags        =  tag *[ ";" tag ]
        tag         =  key [ "=" value ]
        key         =  [ vendor "/" ] 1*( ALPHA / DIGIT / "-" )
        value       =  *valuechar
        valuechar   =  <any octet except NUL, BELL, CR, LF, semicolon (`;`) and SPACE>
        vendor      =  hostname

        prefix      =  servername / ( nickname [ [ "!" user ] "@" host ] )

        command     =  1*letter / 3digit

        params      =  *( SPACE middle ) [ SPACE ":" trailing ]
        nospcrlfcl  =  <any octet except NUL, CR, LF, colon (`:`) and SPACE>
        middle      =  nospcrlfcl *( ":" / nospcrlfcl )
        trailing    =  *( ":" / " " / nospcrlfcl )


        SPACE       =  %x20 *( %x20 )   ; space character(s)
        crlf        =  %x0D %x0A        ; "carriage return" "linefeed"
    *)

    (*
        1. <SPACE> is only ASCII SPACE (' ', 0x20)
        2. After extracting parameters they are all treated equal, trialing is a way to allow SPACE within parameters
        3. NUL (0x00) is not used in the specification but is related to how C handles strings
        4. Last Parameter can be an empty string
    *)

    // Message Format: [@tags] [:source] <command> <parameters>
    // Client should not include a source
    // Servers may send a sourced message, clients must parse it as a regular message
    // Messages are parsed into components tags, prefix, command and parameters

    (*
        Parameters
        params   = * (SPACE middle) [SPACE ":" trailing]
        invalid  = <sequence of any characters except NUL, CR, LF, colon (`:`) and SPACE>
        middle   = invalid *(":" / invalid)
        trailing = *(":" / " " / invalid)
    *)

    (*
        Message Tags Specification
        <message>:        ['@' <tags> <SPACE>] [':' <prefix> <SPACE>] <command> <params> <crlf>
        <tags>:           <tag> [';' <tag>]*
        <tag>:            <key> ['=' <escaped_values>]
        <key>:            [<client_prefix>] [<vendor> '/'] <key_name>
        <client_prefix>:  '+'
        <key_name>:       <non-empty sequence of ascii letters, digits, hyphens ('-')>
        <escaped_values>: <sequence of zero or more utf8 characters except NUL, CR, LF, semicolon (`;`) and SPACE>
        <vendor>:         <host>
    *)

    (*
        Wildcards can be escaped by using the ('\', 0x5C) character, i.e. "\*" or "\?"
        * wildcard is for matching any number of characters
        ? wildcard is for matching one and only one character
    *)

    // Connection Registration
    (*
        Order of commands during registration:
            1. CAP LS 302
            2. PASS maybe
            3. NICK and USER
            4. Capability Negotiation
            5. SASL (if negotiated)
            6. CAP END

        Step 1-3 is sent on connection to server
        If server supports capability negotations then step 4-6 is completed
        Step 3 will end the registration process if the server does not support CAP

        Server will reply on success these messages in order:
            1. RPL_YOURHOST (002)
            2. RPL_CREATED (003)
            3. RPL_MYINFO (004)
            4. At least one RPL_ISUPPORT (005)
            5. There may be other numerics sent here
            6. MOTD or ERR_NOMOTD

        Example of registration response from InspIRCd
        :127.0.0.1 001 testnick :Welcome to the Refsa IRC Network testnick!testuser@127.0.0.1
        :127.0.0.1 002 testnick :Your host is 127.0.0.1, running version InspIRCd-3
        :127.0.0.1 003 testnick :This server was created 23:25:21 Jan 24 2020
        :127.0.0.1 004 testnick 127.0.0.1 InspIRCd-3 iosw biklmnopstv :bklov
        :127.0.0.1 005 testnick AWAYLEN=200 CASEMAPPING=ascii CHANLIMIT=#:20 CHANMODES=b,k,l,imnpst CHANNELLEN=64 CHANTYPES=# ELIST=CMNTU HOSTLEN=64 KEYLEN=32 KICKLEN=255 LINELEN=512 MAXLIST=b:100 :are supported by this server
        :127.0.0.1 005 testnick MAXTARGETS=20 MODES=20 NETWORK=Refsa NICKLEN=30 PREFIX=(ov)@+ SAFELIST STATUSMSG=@+ TOPICLEN=307 USERLEN=10 WHOX :are supported by this server
        :127.0.0.1 251 testnick :There are 0 users and 0 invisible on 1 servers
        :127.0.0.1 253 testnick 1 :unknown connections
        :127.0.0.1 254 testnick 0 :channels formed
        :127.0.0.1 255 testnick :I have 0 clients and 0 servers
        :127.0.0.1 265 testnick :Current local users: 0  Max: 0
        :127.0.0.1 266 testnick :Current global users: 0  Max: 0
        :127.0.0.1 375 testnick :127.0.0.1 message of the day
        :127.0.0.1 372 testnick :-
        :127.0.0.1 372 testnick :-  _____                        _____   _____    _____      _
        :127.0.0.1 372 testnick :- |_   _|                      |_   _| |  __ \  / ____|    | |
        :127.0.0.1 372 testnick :-   | |    _ __    ___   _ __    | |   | |__) || |       __| |
        :127.0.0.1 372 testnick :-   | |   | '_ \  / __| | '_ \   | |   |  _  / | |      / _` |
        :127.0.0.1 372 testnick :-  _| |_  | | | | \__ \ | |_) | _| |_  | | \ \ | |____ | (_| |
        :127.0.0.1 372 testnick :- |_____| |_| |_| |___/ | .__/ |_____| |_|  \_\ \_____| \__,_|
        :127.0.0.1 372 testnick :-     __________________| |_______________________________
        :127.0.0.1 372 testnick :-    |__________________|_|_______________________________|
        :127.0.0.1 372 testnick :-
        :127.0.0.1 372 testnick :-                         Putting the ricer in IRCer since 2007
        :127.0.0.1 372 testnick :-
        :127.0.0.1 372 testnick :-        //\
        :127.0.0.1 372 testnick :-        V  \    WELCOME TO AN INSPIRCD NETWORK
        :127.0.0.1 372 testnick :-         \  \_    If you see this, I am probably new.
        :127.0.0.1 372 testnick :-          \,'.`-.   If I'm not new, my owner is lazy.
        :127.0.0.1 372 testnick :-           |\ `. `.
        :127.0.0.1 372 testnick :-           ( \  `. `-.                        _,.-:\
        :127.0.0.1 372 testnick :-            \ \   `.  `-._             __..--' ,-';/
        :127.0.0.1 372 testnick :-             \ `.   `-.   `-..___..---'   _.--' ,'/
        :127.0.0.1 372 testnick :-              `. `.    `-._        __..--'    ,' /
        :127.0.0.1 372 testnick :-                `. `-_     ``--..''       _.-' ,'
        :127.0.0.1 372 testnick :-                  `-_ `-.___        __,--'   ,'
        :127.0.0.1 372 testnick :-                     `-.__  `----"""    __.-'
        :127.0.0.1 372 testnick :-                          `--..____..--'
        :127.0.0.1 372 testnick :-
        :127.0.0.1 372 testnick :-         ---- To change, see motd.txt.example -----
        :127.0.0.1 372 testnick :-        /                                          \
        :127.0.0.1 372 testnick :-       /   * Web: https://www.inspircd.org          \
        :127.0.0.1 372 testnick :-       |   * IRC: irc.inspircd.org #inspircd        |
        :127.0.0.1 372 testnick :-       |   * Docs: https://docs.inspircd.org        |
        :127.0.0.1 372 testnick :-       |   * Bugs: https://inspircd.org/bugs        |
        :127.0.0.1 372 testnick :-       |                                            |
        :127.0.0.1 372 testnick :-       | We hope you like this software. Please do  |
        :127.0.0.1 372 testnick :-       | make  sure  you  put  some  effort  into   |
        :127.0.0.1 372 testnick :-       | your configuration, though, so you love it.|
        :127.0.0.1 372 testnick :-       | Enjoy.                                     |
        :127.0.0.1 372 testnick :-       |                                            |
        :127.0.0.1 372 testnick :-       \                   -- The InspIRCd Team    /
        :127.0.0.1 372 testnick :-        -------------------------------------------
        :127.0.0.1 372 testnick :-
        :127.0.0.1 376 testnick :End of message of the day.

        Server Features: 

        AWAYLEN=200 CASEMAPPING=ascii CHANLIMIT=#:20 CHANMODES=b,k,l,imnpst CHANNELLEN=64 CHANTYPES=# ELIST=CMNTU HOSTLEN=64 KEYLEN=32 KICKLEN=255 LINELEN=512 MAXLIST=b:100
        MAXTARGETS=20 MODES=20 NETWORK=Refsa NICKLEN=30 PREFIX=(ov)@+ SAFELIST STATUSMSG=@+ TOPICLEN=307 USERLEN=10 WHOX
    *)

    // Capability negotation:
    (*
        CAP REQ, CAP ACK, CAP NAK and CAP END

        Neither server or client needs to support the given CAPs to be able to communicate
        They extend the base implementation of the IRC specification in some way
    *)

    // Client Messages
    (*
        Connection Messages:
            CAP:
                Capability negotiation between server and client
                command: CAP
                parameters: <subcommand> [:<capabilities>]

            AUTHENTICATE
                SASL authentication between client and server
                command: AUTHENTICATE

            PASS:
                Password for connecting
                Must be sent before USER/NICK
                command: PASS
                parameters: <password>

            NICK:
                Gives or changes the nickname of the client
                Can be received from server when a user changes nick, previous name is given in <source> of message
                    Example of nick change from server ":dan-!d@localhost NICK Mamoped"
                command: NICK
                parameters: <nickname>

            USER:
                specifies the username/real name of a new user
                command: USER
                parameters: <username> 0 * <realname>

            OPER:
                Sent by a normal user to obtain operator privileges
                command: OPER
                parameters: <name> <password>

            QUIT:
                Terminates clients connection to server
                command: QUIT
                parameters: [<reason>]

        Channel Messages:
            JOIN:
                attempts to join a given channel
                command: JOIN
                parameters: <channel>[,<channel>] [<key>{,<key>}]
                alt params: 0

                Example messages: 
                JOIN #foobar                    ; join channel #foobar.
                JOIN &foo fubar                 ; join channel &foo using key "fubar".
                JOIN #foo,&bar fubar            ; join channel #foo using key "fubar"
                                                  and &bar using no key.
                JOIN #foo,#bar fubar,foobar     ; join channel #foo using key "fubar".
                                                  and channel #bar using key "foobar".
                JOIN #foo,#bar                  ; join channels #foo and #bar.

            PART:
                Attempts to disconnect from given channel
                command: PART
                parameters: <channel>{,<channel>} [<reason>]

            TOPIC:
                Attempts to change the topic of a channel
                command: TOPIC
                parameters: <channel> [<topic>]

            NAMES:
                attempts to retreive the nicknames of clients connected to a server
                command: NAMES
                parameters: [<channel>{,<channel}]

            LIST:
                attempts to retreive a list of channels
                command: LIST
                parameters: [<channel>{,<channel>}] [<elistcond>{,<elistcond>}]

            MOTD:
                attempts to get the MOTD of the server
                command: MOTD
                parameters: [<target>]

            VERSION:
                attempts to query the version of the server and the RPL_ISUPPORT parameters
                command: VERSION
                parameters: [<target>]

            ADMIN:
                attempts to get admin information of a server or client
                command: ADMIN
                parameters: [<target>]

            TIME:
                attempts to query the local time of the server
                command: TIME
                parameters: [<target>]

            INFO:
                attempts to query information about the server
                command: INFO
                parameters: [<target>]

        Sending Messages:
            PRIVMSG:
                Attempts to send a message to channel or user
                command: PRIVMSG
                parameters: <target>{,<target>} <text to be sent>

                Examples:
                :Angel PRIVMSG Wiz :Hello are you receiving this message ?
                                  ; Message from Angel to Wiz.

                :dan!~h@localhost PRIVMSG #coolpeople :Hi everyone!
                                                ; Message from dan to the channel
                                                  #coolpeople

            NOTICE:
                same as PRIVMSG but wont be replied with automatic messages
                command: NOTICE
                parameters: <target>{,<target>} <text to be sent>
    *)

    // Example Messages
    (*
        ":irc.example.com CAP LS * :multi-prefix extended-join sasl"
        "@id=234AB :dan!d@localhost PRIVMSG #chan :Hey what's up!"
        "CAP REQ :sasl"

        ":irc.example.com CAP * LIST :         ->  ["*", "LIST", ""]"
        "CAP * LS :multi-prefix sasl           ->  ["*", "LS", "multi-prefix sasl"]"
        "CAP REQ :sasl message-tags foo        ->  ["REQ", "sasl message-tags foo"]"
        ":dan!d@localhost PRIVMSG #chan :Hey!  ->  ["#chan", "Hey!"]"
        ":dan!d@localhost PRIVMSG #chan Hey!   ->  ["#chan", "Hey!"]"
    *)

    // RPL_ISUPPORT
    (*
        AWAYLEN:
            Format: AWAYLEN=<number>
            Maximum length for the <reason> of the AWAY command

        CASEMAPPING:
            Format: CASEMAPPING=<casemap>
            indicates the method used by the server to compare equality of case-insensitive strings
                "ascii" a-z = A-Z
                "rfc1459" same as ascii, {}|^ = []\~
                "rfc1459-strict" same as ascii, {}| = []\
                "rfc7613" based on PRECIS to allow UNICODE characters to be casemapped

        CHANLIMIT:
            Format: CHANLIMIT=<prefixes>:[limit]{,<prefixes>:[limit]}
            Indicates the number of channels a client can join
                CHANLIMIT=#:70,&:       ; indicates that clients may join 70 '#' channels and any
                                           number of '&' channels

        CHANMODES:
            Format: CHANMODES=A,B,C,D[,X,Y,...]
            specifies the channel modes available when using the MODE command

        CHANNELLEN:
            Format: CHANNELLEN:<number>
            specifies the maximum length of channel names

        CHANTYPES:
            Format: CHANTYPES=[string]
            Default: CHANTYPES=#
            Specifies the channel prefixes that are available

        ELIST:
            Format: ELIST=<string>
            Indicates that server supports search extensions to LIST command
                C: Searching based on channel creation time
                M: Searching based on mask
                N: Searching based on non-mask
                T: Searching based on topic set time
                U: Searching based on user count in the channel

        EXCEPTS:
            Format: EXCEPTS=[character]
            Empty: e
            indicates the server supports ban exceptions

        EXTBAN:
            Format: EXTBAN=[<prefix>],<types>
            indicates the types of extended ban masks the server supports

        HOSTLEN:
            Format: HOSTLEN=<number>
            Status: Proposed
            Indicates the maximum length that a hostname may be on the server

        INVEX:
            Format: INVEX=[character]
            Indicates the server supports invite exceptions

        KICKLEN:
            Format: KICKLEN=<length>
            Indicates the maximum length of the <reson> for a KICK command

        MAXLIST:
            Format: MAXLIST=<modes>:<limit>{,<modes>:<limit>}
            Specifies how many variable modes of type A that have been defined in the CHANMODES parameter

        MAXTARGETS:
            Format: MAXTARGETS=[number]
            Specifies the maximum number of targets a PRIVMSG or NOTICE command may have

        MODES:
            Format: MODES=[number]
            Specifies how many 'variable' modes may be set on a channel by a single MODE command from a client

        NETWORK:
            Format: NETWORK=<string>
            Indicates the name of the IRC network

        NICKLEN:
            Format: NICKLEN=<number>
            indicates the maximum length of a nickname a client can use

        PREFIX:
            Format: PREFIX=[(modes)prefixes]
            Default: PREFIX=(ov)@+
            prefixes for client statuses within a channel

        SAFELIST:
            Format: SAFELIST
            the server ensures that a client may perform the LIST command without being diconnected 
            due to the large volume of data the LIST command generates.

        SILENCE:
            Format: SILENCE[=<limit>]
            maximum entires a client can have in their silence list

        STATUSMSG:
            Format: STATUSMSG=<string>
            indicates if a server supports clients sending message with PRIVMSG/NOTICE using channel membership prefixes

        TARGMAX:
            Format: TARGMAX=[<command>:[limit]{,<command>:[limit]}]
            defines the maximum number of targets allowed for commands which accept multiple targets

        TOPICLEN:
            Format: TOPICLEN=<number>
            indicates the maximum length of a topic that a client may set ofn a channel

        USERLEN:
            Format: USERLEN=<number>
            indicates the maximum length that a username may be on the server
    *)

    // Character Encodings:
    (*
        Sent messages are encoded as UTF-8
        Received messages first try UTF-8 and use LATIN-1 as a fallback
    *)

    // Message Mapping:
    (*
        Message
            .Tags
            .Source
            .Verb
            .Params
    *)