#load "IRCStreamReader.fsx"
#load "IRCStreamWriter.fsx"

namespace FuncIRC

open System
open System.Threading
open System.Threading.Tasks
open TCPClient
open IRCClientData
open IRCStreamReader
open IRCStreamWriter

#if !DEBUG
module internal IRCClient =
#else
module IRCClient =
#endif
    /// Handles the client connection and disposes it if it loses connection or the cancellation token is invoked
    let ircClientHandler (clientData: IRCClientData) (client: TCPClient) =
        let rec keepAlive() =
            async {
                do! Async.AwaitEvent (clientData.DisconnectClientEvent)

                clientData.TokenSource.Cancel()
                Thread.Sleep (100)

                clientData.Dispose()
                client.Close

                clientData.ClientDisconnected()
            }

        keepAlive()

    /// Starts the TcpClient and connects the NetworkStream to the corresponding reader/writer handlers
    /// Raises <typeref="ClientConnectionException"> if the connection was unsuccessful
    let ircClient (server: string, port: int, useSsl: bool) = 
        let client = new TCPClient (server, port, useSsl)

        match client.Connect with
        | true -> 
            let clientData = new IRCClientData (client)

            // TcpClient
            let tcpClient = (ircClientHandler clientData client)
            // Read Stream
            let readStream = (readStream clientData client)

            let asParallel = Async.Parallel([tcpClient; readStream], 2)
            Async.StartAsTask(asParallel, TaskCreationOptions(), clientData.Token) |> ignore

            clientData
        | false ->
            raise ClientConnectionException