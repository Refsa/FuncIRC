#load "ConnectionClient/IRCClient.fsx"
#load "ConnectionClient/IRCClientData.fsx"
#load "ConnectionClient/IRCStreamReader.fsx"
#load "ConnectionClient/IRCStreamWriter.fsx"
#load "ConnectionClient/Subscription.fsx"
#load "IRC/MessageTypes.fsx"
#load "IRC/MessageHandlers.fsx"

namespace FuncIRC

open IRCClient
open IRCClientData
open IRCStreamReader
open IRCStreamWriter
open Subscription
open MessageTypes
open MessageHandlers
open System.Threading.Tasks

module ClientSetup =

    /// Starts the TcpClient and connects the NetworkStream to the corresponding reader/writer handlers
    let startIrcClient (server: string, port: int) =
        let clientData = ircClient (server, port)

        clientData.MessageSubscriptionEvent 
        |> Event.add (
            fun (m: Message, c: IRCClientData) ->
                match m.Verb.Value.Value with
                | "PING" -> pongMessageHandler (m, c) |> ignore
                | _ -> ()
        )
        
        System.Threading.Thread.Sleep (100)

        clientData