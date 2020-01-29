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
    let internal capMessage = 
        { Tags = None; Source = None; Verb = Some (Verb "CAP"); Params  = Some (toParameters [|"LS"; "302"|]) }

    let internal passMessage pass = 
        { Tags = None; Source = None; Verb = Some (Verb "PASS"); Params = Some (toParameters [|pass|]) }

    let internal nickMessage nick = 
        { Tags = None; Source = None; Verb = Some (Verb "NICK"); Params = Some (toParameters [|nick|]) }

    let internal userMessage user realName =
        { Tags = None; Source = None; Verb = Some (Verb "USER"); Params = Some (toParameters [|user; "0"; "*"; realName|]) }
//#endregion

//#region External message constructs
    let joinChannelMessage channel = { Tags = None; Source = None; Verb = Some (Verb "JOIN"); Params = Some (toParameters [|channel|]) }

    let channelMessage message channel = { Tags = None; Source = None; Verb = Some (Verb "PRIVMSG"); Params = Some (toParameters [|channel; message|]) }
//#endregion

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

        match () with
        | _ when hasNick ->
            match () with
            | _ when userRealNamePass -> UserRealNamePass (nick, user, realName, pass)
            | _ when userRealName     -> UserRealName (nick, user, realName)
            | _ when userPass         -> UserPass (nick, user, pass)
            | _ when hasUser          -> User (nick, user)
            | _                       -> Nick (nick) // Not sure if this should be valid login data
        | _ -> InvalidLoginData

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
                | verb when verb = (NumericsReplies.RPL_WELCOME.ToString()) -> rplWelcomeHandler (m, c) |> ignore
                | verb when verb = (NumericsReplies.RPL_YOURHOST.ToString()) -> rplYourHostHandler (m, c) |> ignore
                | verb when verb = (NumericsReplies.RPL_CREATED.ToString()) -> rplCreatedHandler (m, c) |> ignore
                | verb when verb = (NumericsReplies.RPL_MYINFO.ToString()) -> rplMyInfoHandler (m, c) |> ignore
                | verb when verb = (NumericsReplies.RPL_LUSERCLIENT.ToString()) -> rplLUserClientHandler (m, c) |> ignore
                | verb when verb = (NumericsReplies.RPL_LUSERUNKNOWN.ToString()) -> rplLUserUnknownHandler (m, c) |> ignore
                | verb when verb = (NumericsReplies.RPL_LUSERCHANNELS.ToString()) -> rplLUserChannelsHandler (m, c) |> ignore
                | verb when verb = (NumericsReplies.RPL_LUSERME.ToString()) -> rplLUserMeHandler (m, c) |> ignore
                | verb when verb = (NumericsReplies.RPL_LOCALUSERS.ToString()) -> rplLocalUsersHandler (m, c) |> ignore
                | verb when verb = (NumericsReplies.RPL_GLOBALUSERS.ToString()) -> rplGlobalUsersHandler (m, c) |> ignore
                | _ -> ()
        )

        clientData.AddOutMessages messages

    /// Creates a QUIT messages and adds it to the outbound message queue
    let sendQuitMessage (clientData: IRCClientData) (message: string) =
        { Tags = None; Source = None; Verb = Some (Verb "QUIT"); Params = Some (toParameters [|message|]) }
        |> clientData.AddOutMessage