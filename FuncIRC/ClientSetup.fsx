#load "ConnectionClient/IRCClient.fsx"
#load "ConnectionClient/IRCClientData.fsx"
#load "ConnectionClient/IRCStreamReader.fsx"
#load "ConnectionClient/IRCStreamWriter.fsx"
#load "ConnectionClient/MessageSubscription.fsx"
#load "IRC/MessageTypes.fsx"
#load "IRC/MessageHandlers.fsx"

namespace FuncIRC

open IRCClient
open IRCClientData
open IRCStreamReader
open IRCStreamWriter
open MessageSubscription
open MessageTypes
open MessageHandlers
open System.Threading.Tasks

module ClientSetup =

    /// Starts the TcpClient and connects the NetworkStream to the corresponding reader/writer handlers
    let startIrcClient (server: string, port: int) =
        let clientData = ircClient (server, port)

        clientData.AddSubscription ( MessageSubscription.NewRepeat (Verb ("PING")) pongMessageHandler )

        clientData

    /// Stops the TcpClient and internal stream readers/writers
    let stopIrcClient (clientData: IRCClientData) =
        closeIrcClient clientData