#load "ConnectionClient.fsx"
#load "MessageSubscription.fsx"
#load "IRCStreamReader.fsx"
#load "../IRC/MessageTypes.fsx"
#load "../IRC/VerbHandlers.fsx"

namespace FuncIRC
open System.Threading
open System.Threading.Tasks
open ConnectionClient
open IRCStreamReader
open MessageTypes
open MessageSubscription

module IRCClient =
    /// Handles the client connection and disposes it if it loses connection or the cancellation token is invoked
    let ircClientHandler (client: TCPClient) =
        let cancellationTokenSource = new CancellationTokenSource()
        let rec keepAlive() =
            async {
                Thread.Sleep (10)
                if client.Connected && not cancellationTokenSource.IsCancellationRequested then
                    return! keepAlive()
                else
                    client.Close
            }

        Async.StartAsTask (keepAlive(), TaskCreationOptions(), cancellationTokenSource.Token) |> ignore

        cancellationTokenSource

    /// Cancels the TCPClient connection token and waits for the token WaitHandle to close
    /// Important to include in the client call chain in order to properly dispose of the TcpClient
    let closeIrcClient (token: CancellationTokenSource) =
        token.Cancel()

        let rec waitForClient() =
            async {
                let! waitForCancel = Async.AwaitWaitHandle(token.Token.WaitHandle)
                if waitForCancel then ()
                else return! waitForClient()
            }

        waitForClient() |> Async.RunSynchronously

    /// Starts the IRC TCPClient connection
    /// Raises ClientConnectionException if the connection was unsuccessful
    let ircClient (server: string, port: int) = 
        startClient server port 
        |> fun client ->
            match client with
            | Some client -> 
                let messageSubQueue = MessageSubscriptionQueue()
                setupRequiredIrcMessageSubscriptions messageSubQueue

                let clientTokenSource = ircClientHandler client

                Async.StartAsTask
                    (
                        (readStream client receivedDataHandler messageSubQueue), 
                        TaskCreationOptions(), 
                        clientTokenSource.Token
                    ) |> ignore

                (client, clientTokenSource, messageSubQueue)
            | None -> raise ClientConnectionException