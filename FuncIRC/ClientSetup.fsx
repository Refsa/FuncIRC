#load "ConnectionClient/IRCClient.fsx"
#load "ConnectionClient/IRCStreamReader.fsx"
#load "ConnectionClient/IRCStreamWriter.fsx"
#load "ConnectionClient/MessageSubscription.fsx"
#load "IRC/MessageTypes.fsx"

namespace FuncIRC

open IRCClient
open IRCStreamReader
open IRCStreamWriter
open MessageSubscription
open MessageTypes
open System.Threading.Tasks

module ClientSetup =

    let startIrcClient (server: string, port: int) =
        let clientData = ircClient (server, port)

        clientData.SubscriptionQueue.AddSubscription (MessageSubscription.NewRepeat (Verb ("PING")) (fun (m) -> printfn "PONG"; clientData.OutQueue.AddMessage { Tags = None; Source = None; Verb = Some (Verb "PONG"); Params = None }; None))

        // Read Stream
        Async.StartAsTask
            (
                (readStream clientData), TaskCreationOptions(), clientData.TokenSource.Token
            ) |> ignore

        // Write Stream
        Async.StartAsTask
            (
                (writeStream clientData), TaskCreationOptions(), clientData.TokenSource.Token
            ) |> ignore

        clientData