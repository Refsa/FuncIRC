#load "../IRC/Handlers/ServerFeatureHandlers.fsx"
#load "IRCStreamReader.fsx"
#load "IRCStreamWriter.fsx"

namespace FuncIRC

open System
open System.Threading
open System.Threading.Tasks
open TCPClient
open IRCClient
open IRCStreamReader
open IRCStreamWriter
open ServerFeatureHandlers

#if !DEBUG
module internal IRCClientHandler =
#else
module IRCClientHandler =
#endif

    /// <summary>
    /// Client wasn't connected successfully
    /// </summary>
    exception ClientConnectionException

    /// <summary>
    /// Handles the client connection and disposes it if it loses connection or the cancellation token is invoked
    /// </summary>
    let ircClientHandler (clientData: IRCClient) (client: TCPClient) =
        let rec keepAlive() =
            async {
                do! Async.AwaitEvent (clientData.DisconnectClientEvent)

                clientData.TokenSource.Cancel()
                Thread.Sleep (100)

                clientData.Dispose()
                client.Close
                //(serverFeaturesProcessor :> IDisposable).Dispose()

                clientData.ClientDisconnected()
            }

        keepAlive()

    /// <summary>
    /// Starts the TcpClient and connects the Stream to the corresponding reader/writer handlers
    /// Raises <typeref="ClientConnectionException"> if the connection was unsuccessful
    /// </summary>
    let startIrcClient (server: string, port: int, useSsl: bool) = 
        let client = new TCPClient (server, port, useSsl)

        let connected = client.Connect

        match connected with
        | true -> 
            let clientData = new IRCClient (client, true)

            // TcpClient
            let tcpClient = (ircClientHandler clientData client)
            // Read Stream
            let readStream = (readStream clientData client)

            let asParallel = Async.Parallel([tcpClient; readStream], 2)
            Async.StartAsTask(asParallel, TaskCreationOptions(), clientData.Token) |> ignore

            clientData
        | false ->
            raise ClientConnectionException