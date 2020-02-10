#load "../Networking/IRCClientHandler.fsx"
#load "../Networking/IRCClient.fsx"
#load "../Networking/IRCStreamReader.fsx"
#load "../Networking/IRCStreamWriter.fsx"
#load "../IRC/Types/MessageTypes.fsx"
#load "../IRC/Handlers/MessageHandlers.fsx"

namespace FuncIRC

open IRCClientHandler
open MessageTypes

module ClientSetup =

    /// <summary>
    /// Creates the server connection and adds required internal message subscriptions
    /// </summary>
    let startIrcClient (server: string, port: int, useSsl: bool) =
        let clientData = 
            match useSsl with
            | true -> startIrcClient (server, 6697, useSsl)
            | false -> 
                match port with
                | 6697 -> printfn "SSL port was used on a non-SSL connection"; startIrcClient (server, 6667, useSsl)
                | _ -> startIrcClient (server, port, useSsl)

        clientData.MessageSubscriptionEvent 
        |> Event.add (
            fun (m: Message, c: IRCClient) ->
                match m.Verb.Value.Value with
                | "PING" -> pongMessageHandler (m, c) |> ignore
                | _ -> ()
        )

        /// TODO: Add a sync await here
        System.Threading.Thread.Sleep (100)

        clientData