#load "ConnectionClient.fsx"
#load "IRCStreamReader.fsx"

namespace FuncIRC
open System.Threading
open System.Threading.Tasks
open System.Text

module IRCClient =
    open ConnectionClient
    open IRCStreamReader

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

    let sendIrcMessage (client: Client) (message: string) =
        let messageData =
            match message with
            | message when message.EndsWith("\r\n") -> Encoding.UTF8.GetBytes (message)
            | _ -> Encoding.UTF8.GetBytes (message + "\r\n")

        client.Stream.Write (messageData, 0, messageData.Length)

    let sendRegistrationMessage (client: Client) (nick: string, user: string, realName: string, pass: string) =
        let message = 
            match () with
            | _ when pass <> "" && nick <> "" && user <> "" -> "CAP LS 302\r\nPASS " + pass + "\r\nNICK " + nick + "\r\nUSER 0 * " + user + "\r\n"
            | _ when nick <> "" && user <> "" -> "CAP LS 302\r\nNICK " + nick + "\r\nUSER " + user + " 0 * " + realName + "\r\n"
            | _ -> raise RegistrationContentException

        sendIrcMessage client message