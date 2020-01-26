#load "../ConnectionClient/IRCStreamWriter.fsx"
#load "MessageTypes.fsx"

namespace FuncIRC

open IRCStreamWriter
open MessageTypes

module MessageHandlers =
    
    let pongMessageHandler (client, message) =
        printfn "PONG"
        client |> sendIrcMessage <| "PONG"

    let rplWelcomeHandler (client, message: Message) =
        printfn "RPL_WELCOME: %s" message.Params.Value.Value.[1].Value