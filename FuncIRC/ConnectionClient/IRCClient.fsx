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
open MessageSubscription
open MessageQueue
open IRCStreamReader
open IRCStreamWriter

module internal IRCClient =
    let private streamWriteInterval        = 10
    let private tcpClientKeepAliveInterval = 50
    let private cancelAwaitTime            = streamWriteInterval + tcpClientKeepAliveInterval

    /// Handles the client connection and disposes it if it loses connection or the cancellation token is invoked
    let ircClientHandler (client: TCPClient) (token: CancellationToken) =
        let rec keepAlive() =
            async {
                Thread.Sleep (tcpClientKeepAliveInterval)

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

                token.CancelAfter(cancelAwaitTime)
                Thread.Sleep (cancelAwaitTime * 2)

                token.Dispose()
            }

        waitForClient() |> Async.RunSynchronously

    /// Starts the IRC TCPClient connection
    /// Raises <typeref = "ClientConnectionException"> if the connection was unsuccessful
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
                let tcpClient = (ircClientHandler client clientCancellationTokenSource.Token)

                // Read Stream
                let readStream = (readStream clientData client.Stream)

                // Write Stream
                let writeStream = (writeStream clientData client.Stream streamWriteInterval)

                let asParallel = Async.Parallel([tcpClient; readStream; writeStream], 3)

                Async.StartAsTask(asParallel, TaskCreationOptions(), clientCancellationTokenSource.Token) |> ignore

                clientData
            | None -> raise ClientConnectionException
