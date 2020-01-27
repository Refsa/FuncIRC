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
        None

    let rplYourHostHandler (message: Message) =
        printfn "RPL_YOURHOST: %s" message.Params.Value.Value.[1].Value
        None

    let rplCreatedHandler (message: Message) =
        printfn "RPL_CREATED: %s" message.Params.Value.Value.[1].Value
        None

    let rplMyInfoHandler (message: Message) =
        printfn "RPL_MYINFO: "
        None