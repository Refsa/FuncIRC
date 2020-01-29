#load "../ConnectionClient/MessageSubscription.fsx"
#load "../ConnectionClient/IRCClientData.fsx"
#load "MessageTypes.fsx"
#load "NumericReplies.fsx"

namespace FuncIRC

open MessageTypes
open MessageSubscription
open IRCClientData
open NumericReplies

module internal MessageHandlers =
    let private pongMessage = Message.NewSimpleMessage (Some (Verb "PONG")) None
    let pongMessageHandler (message: Message, clientData: IRCClientData) =
        clientData.AddOutMessage pongMessage
#if DEBUG
        Some {Verb = "MSG_PONG"; Content = "PONG"}
#endif

    let rplWelcomeHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_WELCOME: %s" message.Params.Value.Value.[1].Value
#if DEBUG
        Some {Verb = "RPL_WELCOME"; Content = message.Params.Value.Value.[1].Value.Split(' ') |> Array.last}
#endif

    let rplYourHostHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_YOURHOST: %s" message.Params.Value.Value.[1].Value
#if DEBUG
        None
#endif

    let rplCreatedHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_CREATED: %s" message.Params.Value.Value.[1].Value
#if DEBUG
        None
#endif

    let rplMyInfoHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_MYINFO: "
#if DEBUG
        None
#endif

    let rplLUserClientHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_LUSERCLIENT: %A" [| for p in message.Params.Value.Value -> p.Value |]
#if DEBUG
        None
#endif

    let rplLUserUnknownHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_LUSERUNKNOWN: %A" [| for p in message.Params.Value.Value -> p.Value |]
#if DEBUG
        None
#endif

    let rplLUserChannelsHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_LUSERCHANNELS: %A" [| for p in message.Params.Value.Value -> p.Value |]
#if DEBUG
        None
#endif

    let rplLUserMeHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_LUSERME: %A" [| for p in message.Params.Value.Value -> p.Value |]
#if DEBUG
        None
#endif

    let rplLocalUsersHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_LOCALUSERS: %A" [| for p in message.Params.Value.Value -> p.Value |]
#if DEBUG
        None
#endif

    let rplGlobalUsersHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_GLOBALUSERS: %A" [| for p in message.Params.Value.Value -> p.Value |]
#if DEBUG
        None
#endif