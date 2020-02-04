#load "IRCClientData.fsx"
#load "MessageQueue.fsx"
#load "../IRC/MessageTypes.fsx"
#load "../IRC/NumericReplies.fsx"
#load "../IRC/MessageHandlers.fsx"

namespace FuncIRC

open IRCClientData
open MessageTypes
open MessageHandlers
open MessageQueue
open NumericReplies

module IRCMessages =
    /// Exception thrown when parameters to a registration message was missing
    exception RegistrationContentException

//#region Internal Message Constructs
    /// Creates the intial CAP message sent on registration
    let internal capMessage = 
        { Tags = None; Source = None; Verb = Some (Verb "CAP"); Params  = Some (toParameters [| "LS"; "302" |]) }

    /// Creates a PASS message with the given pass string
    let internal passMessage pass = 
        { Tags = None; Source = None; Verb = Some (Verb "PASS"); Params = Some (toParameters [| pass |]) }

    /// Creates a NICK message with the given nick string
    let internal nickMessage nick = 
        { Tags = None; Source = None; Verb = Some (Verb "NICK"); Params = Some (toParameters [| nick |]) }

    /// Creates a USER message with the given user and realName strings
    let internal userMessage user realName =
        { Tags = None; Source = None; Verb = Some (Verb "USER"); Params = Some (toParameters [| user; "0"; "*"; realName |]) }

    /// Creates an AWAY message with the given awayMessage string
    let internal createAwayMessage awayMessage =
        { Tags = None; Source = None; Verb = Some (Verb "AWAY"); Params = Some (toParameters [| awayMessage |]) }

    /// Creates a QUIT message with the given quitMessage string
    let internal createQuitMessage quitMessage =
        { Tags = None; Source = None; Verb = Some (Verb "QUIT"); Params = Some (toParameters [| quitMessage |]) }

    /// Creates a KICK message with the given kickTarget and kickMessage strings
    let internal createKickMessage kickTarget kickMessage =
        { Tags = None; Source = None; Verb = Some (Verb "KICK"); Params = Some (toParameters [| kickTarget; ":" + kickMessage |]) }

    /// Creates a TOPIC message with the given topic string
    let internal createTopicMessage topic =
        { Tags = None; Source = None; Verb = Some (Verb "TOPIC"); Params = Some (toParameters [| topic |]) }

    /// Creates a JOIN message on the given channel
    let internal createJoinChannelMessage channel = { Tags = None; Source = None; Verb = Some (Verb "JOIN"); Params = Some (toParameters [|channel|]) }

    /// Creates a PRIVMSG message with the given message string on the given channel
    let internal createChannelPrivMessage message channel = { Tags = None; Source = None; Verb = Some (Verb "PRIVMSG"); Params = Some (toParameters [|channel; ":" + message|]) }
//#endregion

//#region Validators
    /// Validates the channel string
    let validateChannel (clientData: IRCClientData) (channel: string) =
        if      channel = "" then false
        else if channel.Length > clientData.GetServerInfo.MaxChannelLength then false
        else

        let channelPrefix = channel.[0]

        if Map.containsKey channelPrefix clientData.GetServerInfo.ChannelPrefixes |> not then false
        else true

    /// Validates the nick string
    let validateNick (clientData: IRCClientData) (nick: string) =
        if nick = "" then false
        else if nick.IndexOf (' ') <> -1 then false
        else if nick.Length > clientData.GetServerInfo.MaxNickLength then false
        else true

    /// Validates the user string
    let validateUser (clientData: IRCClientData) (user: string) =
        if user = "" then false
        else if user.Length > clientData.GetServerInfo.MaxUserLength then false
        else true

    /// Validates the topic string
    let validateTopic (clientData: IRCClientData) (topic: string) =
        if      topic = "" then false
        else if topic.Length > clientData.GetServerInfo.MaxTopicLength then false
        else true
//#endregion Validators

    /// Active Pattern to validate and check which part of the login details that were given
    /// <returns>
    /// <param name="UserRealNamePass">tuple (nick, user, realName, pass)</param>
    /// <param name="UserRealName">tuple (nick, user, realName)</param>
    /// <param name="UserPass">tuple (nick, user, pass)</param>
    /// <param name="User">tuple (nick, user)</param>
    /// <param name="Nick">tuple (nick)</param>
    /// </returns>
    let (|UserRealNamePass|UserRealName|UserPass|User|Nick|InvalidLoginData|) (loginData: string * string * string * string) =
        let nick, user, realName, pass = loginData
        let hasRealName = realName <> ""
        let hasNick     = nick <> ""
        let hasUser     = user <> ""
        let hasPass     = pass <> ""

        let userRealNamePass = hasUser && hasRealName && hasPass
        let userRealName     = hasUser && hasRealName
        let userPass         = hasUser && hasPass

        match hasNick with
        | true ->
            match () with
            | _ when userRealNamePass -> UserRealNamePass (nick, user, realName, pass)
            | _ when userRealName     -> UserRealName (nick, user, realName)
            | _ when userPass         -> UserPass (nick, user, pass)
            | _ when hasUser          -> User (nick, user)
            | _                       -> Nick (nick) // Not sure if this should be valid login data
        | false -> InvalidLoginData

    /// Creates a registration message and sends it to the outbound message queue
    /// Subscribes to incoming VERBs related to the registration message
    /// <param name="loginData"> tuple formatted as nick, user, realName, pass </param>
    let sendRegistrationMessage (clientData: IRCClientData) (loginData: string * string * string * string) =
        let messages = 
            match loginData with
            | InvalidLoginData -> raise RegistrationContentException
            | UserRealNamePass (nick, user, realName, pass) -> [ capMessage; passMessage pass; nickMessage nick; userMessage user realName ]
            | UserPass (nick, user, pass)                   -> [ capMessage; passMessage pass; nickMessage nick; userMessage user user ]
            | UserRealName (nick, user, realName)           -> [ capMessage; nickMessage nick; userMessage user realName ]
            | User (nick, user)                             -> [ capMessage; nickMessage nick; userMessage user user ]
            | Nick (nick)                                   -> [ capMessage; nickMessage nick; userMessage nick nick ]

        clientData.MessageSubscriptionEvent
        |> Event.add (
            fun (m, c) -> 
                match m.Verb.Value.Value with
                | verb when verb = (NumericsReplies.RPL_WELCOME.ToString())       -> rplWelcomeHandler (m, c)
                | verb when verb = (NumericsReplies.RPL_YOURHOST.ToString())      -> rplYourHostHandler (m, c)
                | verb when verb = (NumericsReplies.RPL_CREATED.ToString())       -> rplCreatedHandler (m, c)
                | verb when verb = (NumericsReplies.RPL_MYINFO.ToString())        -> rplMyInfoHandler (m, c)
                | verb when verb = (NumericsReplies.RPL_LUSERCLIENT.ToString())   -> rplLUserClientHandler (m, c)
                | verb when verb = (NumericsReplies.RPL_LUSERUNKNOWN.ToString())  -> rplLUserUnknownHandler (m, c)
                | verb when verb = (NumericsReplies.RPL_LUSERCHANNELS.ToString()) -> rplLUserChannelsHandler (m, c)
                | verb when verb = (NumericsReplies.RPL_LUSERME.ToString())       -> rplLUserMeHandler (m, c)
                | verb when verb = (NumericsReplies.RPL_LOCALUSERS.ToString())    -> rplLocalUsersHandler (m, c)
                | verb when verb = (NumericsReplies.RPL_GLOBALUSERS.ToString())   -> rplGlobalUsersHandler (m, c)
                | _ -> ()
        )

        clientData.AddOutMessages messages

    /// Creates a JOIN message to join a channel
    /// <returns> True if the message was added to the outbound message, false if not </returns>
    let sendJoinMessage (clientData: IRCClientData) (channel: string) =
        match validateChannel clientData channel with
        | false -> false
        | true -> 
            createJoinChannelMessage channel |> clientData.AddOutMessage
            true

    /// Creates a PRIVMSG with channel as target using the given message
    /// TODO: Remove if branches
    /// <returns> True if the message was added to the outbound message, false if not </returns>
    let sendChannelPrivMsg (clientData: IRCClientData) (channel: string) (message: string) =
        if      message = "" then false
        else if message.Length > (clientData.GetServerInfo.LineLength - clientData.GetServerInfo.MaxHostLength) then false
        else

        match validateChannel clientData channel with
        | false -> false
        | true ->
            createChannelPrivMessage channel message |> clientData.AddOutMessage
            true

    /// Creates a kick message and adds it to the outbound messages
    /// TODO: Remove if branches
    /// <returns> True if the message was added to the outbound message, false if not </returns>
    let sendKickMessage (clientData: IRCClientData) (kickUser: string) (message: string) =
        if validateUser clientData kickUser |> not then false
        else if message = "" || message.Length > clientData.GetServerInfo.MaxKickLength then false
        else

        createKickMessage kickUser message |> clientData.AddOutMessage
        true

    /// Creates a topic message and adds it to the outbound messages
    /// <returns> True if the message was added to the outbound message, false if not </returns>
    let sendTopicMessage (clientData: IRCClientData) (topic: string) =
        match validateTopic clientData topic with
        | false -> false
        | true ->
            createTopicMessage topic |> clientData.AddOutMessage
            true

    /// Creates a QUIT messages and adds it to the outbound message queue
    /// <returns> True if the message was added to outqueue, false if not </returns>
    let sendQuitMessage (clientData: IRCClientData) (message: string) =
        match message.Length > 200 with
        | true -> false
        | false ->
            createQuitMessage message |> clientData.AddOutMessage
            true

    /// Creates an AWAY messages and adds it to the outboid message queue if the length of the message was within bounds
    /// <returns> True if the message was added to outqueue, false if not </returns>
    let sendAwayMessage (clientData: IRCClientData) (message: string) =
        match message.Length > clientData.GetServerInfo.MaxAwayLength with
        | true  -> false
        | false -> 
            createAwayMessage message |> clientData.AddOutMessage
            true