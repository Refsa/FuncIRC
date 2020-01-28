#load "MessageSubscription.fsx"
#load "MessageQueue.fsx"
#load "IRCClientData.fsx"
#load "../IRC/NumericReplies.fsx"
#load "../IRC/MessageTypes.fsx"
#load "../IRC/MessageHandlers.fsx"

namespace FuncIRC

open IRCClientData
open MessageSubscription
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
    let sendIrcMessage (stream: NetworkStream) (message: string) =
        let messageData =
            match message with
            | message when message.EndsWith("\r\n") -> Encoding.UTF8.GetBytes (message)
            | _ -> Encoding.UTF8.GetBytes (message + "\r\n")

        try
            printfn "Sending Message(s):\n\t%s" (message.Replace("\r\n", "\n\t"))
            stream.Write (messageData, 0, messageData.Length) 
        with
            | e -> printfn "Exception when writing message(s) to stream: %s" e.Message

    /// Construct a single outboud IRC message from a list of messages
    let messagesToString (messages: Message list) =
        let mutable outboundMessage = ""
        messages |> List.iter (fun (m: Message) -> outboundMessage <- outboundMessage + m.ToMessageString + "\r\n" )
        outboundMessage

    /// Writes the message to stream
    let writeMessageToStream (message: Message) (stream: NetworkStream) =
        let writeMessage() =
            async {
                stream |> sendIrcMessage <| message.ToMessageString
            }

        Async.Start (writeMessage())

    /// Run until there are new messages in the outbound queue
    let rec getOutboudMessages (clientData: IRCClientData) =
        async {
            match clientData.OutQueue.Count with
            | 0 -> 
                Thread.Sleep(clientData.StreamWriteInterval); 
                return! getOutboudMessages clientData
            | 1 -> 
                return clientData.OutQueue.PopMessage.ToMessageString
            | _ -> 
                return messagesToString clientData.OutQueue.PopMessages
        }

    /// Contains an internal async loop that looks at clientData.OutQueue and sends the messages
    /// Has a 10ms sleep duration between each message sent 
    /// will send multiple messages at the same time
    let writeStream (clientData: IRCClientData) (stream: NetworkStream) (writeInterval: int) =
        let rec writeLoop() =
            async {
                let! outboundMessage = getOutboudMessages(clientData)

                sendIrcMessage stream outboundMessage

                Thread.Sleep (writeInterval)

                match clientData.TokenSource.IsCancellationRequested with
                | false -> return! writeLoop()
                | true -> ()
            }

        writeLoop()