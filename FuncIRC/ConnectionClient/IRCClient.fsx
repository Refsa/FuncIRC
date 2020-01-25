#load "ConnectionClient.fsx"

namespace FuncIRC
open System.Threading
open System.Threading.Tasks

module IRCClient =
    open ConnectionClient

    exception RegistrationContentException

    /// Starts the IRC Client connection
    /// Raises ClientConnectionException if the connection was unsuccessful
    let ircClient (server: string) (port: int) = 
        startClient server port 
        |> fun client ->
            if client.IsSome then
                client.Value
            else
                raise ClientConnectionException

    /// Handles the client connection and disposes it if it loses connection or the cancellation token is invoked
    let ircClientHandler (client: Client) =
        let cancellationTokenSource = new CancellationTokenSource()
        let rec keepAlive() =
            async {
                Thread.Sleep (1)
                if client.Connected && not cancellationTokenSource.IsCancellationRequested then
                    return! keepAlive()
                else
                    client.Close
            }

        Async.StartAsTask (keepAlive(), TaskCreationOptions(), cancellationTokenSource.Token) |> ignore
        (client, cancellationTokenSource)

    /// Cancels the Client connection token and waits for the token WaitHandle to close
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