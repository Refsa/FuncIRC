#load "ConnectionClient.fsx"
#load "MessageSubscription.fsx"
#load "../IRC/MessageTypes.fsx"
#load "IRCClientData.fsx"
#load "MessageQueue.fsx"
#load "IRCStreamReader.fsx"
#load "IRCStreamWriter.fsx"

namespace FuncIRC
open System.Text
open System.Threading
open System.Threading.Tasks
open ConnectionClient
open IRCClientData
open MessageSubscription
open MessageQueue
open IRCStreamReader
open IRCStreamWriter

module internal IRCClient =
    /// Handles the client connection and disposes it if it loses connection or the cancellation token is invoked
    let ircClientHandler (client: TCPClient) (token: CancellationToken) =
        let rec keepAlive() =
            async {
                Thread.Sleep (50)

                if client.Connected && not token.IsCancellationRequested then
                    return! keepAlive()
                else
                    client.Close
            }

        keepAlive()

    /// Cancels the TCPClient connection token and waits for the token WaitHandle to close
    /// Important to include in the client call chain in order to properly dispose of the TcpClient
    let closeIrcClient (token: CancellationTokenSource) =
        let rec waitForClient() =
            async {
                token.CancelAfter(100)
                Thread.Sleep (1000)

                token.Dispose()
            }

        waitForClient() |> Async.RunSynchronously

    /// Starts the IRC TCPClient connection
    /// Raises ClientConnectionException if the connection was unsuccessful
    let ircClient (server: string, port: int) = 
        startClient server port 
        |> fun client ->
            match client with
            | Some client -> 
                let clientCancellationTokenSource = new CancellationTokenSource()

                let clientData =
                    {   
                        TokenSource = clientCancellationTokenSource
                        SubscriptionQueue = MessageSubscriptionQueue()
                        OutQueue = MessageQueue()
                        InQueue = MessageQueue()
                    }

                // TcpClient
                Async.StartAsTask
                    (
                        (ircClientHandler client clientCancellationTokenSource.Token), TaskCreationOptions(), clientCancellationTokenSource.Token
                    ) |> ignore

                // Read Stream
                Async.StartAsTask
                    (
                        (readStream clientData client.Stream), TaskCreationOptions(), clientCancellationTokenSource.Token
                    ) |> ignore

                // Write Stream
                Async.StartAsTask
                    (
                        (writeStream clientData client.Stream), TaskCreationOptions(), clientCancellationTokenSource.Token
                    ) |> ignore

                clientData
            | None -> raise ClientConnectionException
