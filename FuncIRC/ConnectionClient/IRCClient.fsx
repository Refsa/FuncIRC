#load "ConnectionClient.fsx"

namespace FuncIRC
open System.Threading
open System.Threading.Tasks

module IRCClient =
    open ConnectionClient

    exception RegistrationContentException

    let ircClient (server: string) (port: int) = 
        startClient server port 
        |> fun client ->
            if client.IsSome then
                client.Value
            else
                raise ClientConnectionException

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

    let closeIrcClient (token: CancellationTokenSource) =
        token.Cancel()

        let rec waitForClient() =
            async {
                let! waitForCancel = Async.AwaitWaitHandle(token.Token.WaitHandle)
                if waitForCancel then ()
                else return! waitForClient()
            }

        waitForClient() |> Async.RunSynchronously