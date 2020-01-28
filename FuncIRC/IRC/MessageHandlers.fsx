#load "../ConnectionClient/MessageSubscription.fsx"
#load "MessageTypes.fsx"

namespace FuncIRC

open MessageTypes
open MessageSubscription

module internal MessageHandlers =
    let pongMessageHandler (message: Message) =
        Some (MessageResponse.NewMessage "PONG")

    let rplWelcomeHandler (message: Message) =
        printfn "RPL_WELCOME: %s" message.Params.Value.Value.[1].Value
        Some {ResponseType = ResponseType.ClientInfoSelf; Content = message.Params.Value.Value.[1].Value.Split(' ') |> Array.last}

    let rplYourHostHandler (message: Message) =
        printfn "RPL_YOURHOST: %s" message.Params.Value.Value.[1].Value
        None

    let rplCreatedHandler (message: Message) =
        printfn "RPL_CREATED: %s" message.Params.Value.Value.[1].Value
        None

    let rplMyInfoHandler (message: Message) =
        printfn "RPL_MYINFO: "
        None

    let rplLUserClientHandler (message: Message) =
        printfn "RPL_LUSERCLIENT: %A" [| for p in message.Params.Value.Value -> p.Value |]
        None

    let rplLUserUnknownHandler (message: Message) =
        printfn "RPL_LUSERUNKNOWN: %A" [| for p in message.Params.Value.Value -> p.Value |]
        None

    let rplLUserChannelsHandler (message: Message) =
        printfn "RPL_LUSERCHANNELS: %A" [| for p in message.Params.Value.Value -> p.Value |]
        None

    let rplLUserMeHandler (message: Message) =
        printfn "RPL_LUSERME: %A" [| for p in message.Params.Value.Value -> p.Value |]
        None

    let rplLocalUsersHandler (message: Message) =
        printfn "RPL_LOCALUSERS: %A" [| for p in message.Params.Value.Value -> p.Value |]
        None

    let rplGlobalUsersHandler (message: Message) =
        printfn "RPL_GLOBALUSERS: %A" [| for p in message.Params.Value.Value -> p.Value |]
        None