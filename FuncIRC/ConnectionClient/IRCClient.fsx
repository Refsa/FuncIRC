#load "ConnectionClient.fsx"
#load "MessageSubscription.fsx"
#load "../IRC/MessageTypes.fsx"
#load "IRCClientData.fsx"
#load "MessageQueue.fsx"
#load "IRCStreamReader.fsx"
#load "IRCStreamWriter.fsx"

namespace FuncIRC
open System.Threading
open System.Threading.Tasks
open ConnectionClient
open IRCClientData
open IRCStreamReader
open IRCStreamWriter

module internal IRCClient =
    /// Handles the client connection and disposes it if it loses connection or the cancellation token is invoked
    let ircClientHandler (clientData: IRCClientData) (client: TCPClient) =
        let rec keepAlive() =
            async {
                Thread.Sleep (clientData.TcpClientKeepAliveInterval)

                if client.Connected && not clientData.Token.IsCancellationRequested then
                    return! keepAlive()
                else
                    client.Close
                    clientData.ClientDisconnected()
            }

        keepAlive()

    /// Cancels the TCPClient connection token and waits for the token WaitHandle to close
    /// Important to include in the client call chain in order to properly dispose of the TcpClient
    let closeIrcClient (clientData: IRCClientData) =
        let rec waitForClient() =
            async {
                clientData.TokenSource.CancelAfter(clientData.CancelAwaitTime)
                Thread.Sleep (clientData.CancelAwaitTime * 2)

                clientData.TokenSource.Dispose()
            }

        waitForClient() |> Async.RunSynchronously

    /// Starts the IRC TCPClient connection
    /// Raises <typeref="ClientConnectionException"> if the connection was unsuccessful
    let ircClient (server: string, port: int) = 
        let client = startClient server port

        match client.Connect with
        | true -> 
            let clientData = IRCClientData()
                
            // TcpClient
            let tcpClient = (ircClientHandler clientData client)
            // Read Stream
            let readStream = (readStream clientData client.Stream)
            // Write Stream
            let writeStream = (writeStream clientData client.Stream clientData.StreamWriteInterval)

            let asParallel = Async.Parallel([tcpClient; readStream; writeStream], 3)
            Async.StartAsTask(asParallel, TaskCreationOptions(), clientData.Token) |> ignore

            clientData
        | false ->
            raise ClientConnectionException
