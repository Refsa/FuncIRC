#load "TCPClient.fsx"
#load "../Utils/MessageQueue.fsx"
#load "../Client/IRCClientData.fsx"
#load "../IRC/Types/NumericReplies.fsx"
#load "../IRC/Types/MessageTypes.fsx"
#load "../IRC/Handlers/MessageHandlers.fsx"

namespace FuncIRC

open TCPClient
open IRCClientData
open MessageHandlers
open MessageTypes
open MessageQueue
open IRCClientData
open System.Text
open System.Threading
open System.Net.Sockets

module internal IRCStreamWriter =
    /// Verifies and sends the message string to the client stream
    /// Preferred syntax for sending messages is client |> sendIrcMessage <| message
    let sendIrcMessage (client: TCPClient) (message: string) =
        let messageData =
            match message with
            | message when message.EndsWith("\r\n") -> Encoding.UTF8.GetBytes (message)
            | _ -> Encoding.UTF8.GetBytes (message + "\r\n")

        try
            printfn "Sending Message(s):\n\t%s" (message.Replace("\r\n", "\n\t"))
            client.WriteToStream messageData
        with
            | e -> printfn "Exception when writing message(s) to stream: %s" e.Message

    /// Contains an internal async loop that looks at clientData.OutQueue and sends the messages
    /// Has a 10ms sleep duration between each message sent 
    /// will send multiple messages at the same time
    let writeStream (clientData: IRCClientData) (client: TCPClient) =
        let rec writeLoop() =
            async {
                do! Async.AwaitEvent (clientData.SendMessageEvent)

                clientData.GetOutboundMessages
                |> function
                | m when m = "" -> ()
                | m -> client |> sendIrcMessage <| m

                match clientData.TokenSource.IsCancellationRequested with
                | false -> return! writeLoop()
                | true -> ()
            }

        writeLoop()