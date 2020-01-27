#load "ConnectionClient/IRCClient.fsx"
#load "ConnectionClient/IRCClientData.fsx"
#load "ConnectionClient/IRCStreamReader.fsx"
#load "ConnectionClient/IRCStreamWriter.fsx"
#load "ConnectionClient/MessageSubscription.fsx"
#load "IRC/MessageTypes.fsx"

namespace FuncIRC

open IRCClient
open IRCClientData
open IRCStreamReader
open IRCStreamWriter
open MessageSubscription
open MessageTypes
open System.Threading.Tasks

module ClientSetup =

    /// Starts the TcpClient and connects the NetworkStream to the corresponding reader/writer handlers
    let startIrcClient (server: string, port: int) =
        let clientData = ircClient (server, port)

        clientData.SubscriptionQueue.AddSubscription (MessageSubscription.NewRepeat (Verb ("PING")) (fun (m) -> clientData.OutQueue.AddMessage { Tags = None; Source = None; Verb = Some (Verb "PONG"); Params = None }; None))

        clientData

    /// Stops the TcpClient and internal stream readers/writers
    let stopIrcClient (clientData: IRCClientData) =
        closeIrcClient clientData.TokenSource