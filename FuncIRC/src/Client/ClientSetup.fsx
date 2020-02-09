#load "TCPClient/IRCClientHandler.fsx"
#load "TCPClient/IRCClient.fsx"
#load "TCPClient/IRCStreamReader.fsx"
#load "TCPClient/IRCStreamWriter.fsx"
#load "IRC/MessageTypes.fsx"
#load "IRC/MessageHandlers.fsx"

namespace FuncIRC

open IRCClientHandler
open IRCClient
open MessageTypes
open MessageHandlers

module ClientSetup =
    /// Creates the server connection and adds required internal message subscriptions
    let startIrcClient (server: string, port: int, useSsl: bool) =
        let clientData = 
            match useSsl with
            | true -> ircClient (server, 6697, useSsl)
            | false -> 
                match port with
                | 6697 -> printfn "SSL port was used on a non-SSL connection"; ircClient (server, 6667, useSsl)
                | _ -> ircClient (server, port, useSsl)

        clientData.MessageSubscriptionEvent 
        |> Event.add (
            fun (m: Message, c: IRCClient) ->
                match m.Verb.Value.Value with
                | "PING" -> pongMessageHandler (m, c) |> ignore
                | _ -> ()
        )

        System.Threading.Thread.Sleep (100)

        clientData