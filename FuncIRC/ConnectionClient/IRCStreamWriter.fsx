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
            printfn "Sending Message: %s" message
            stream.Write (messageData, 0, messageData.Length) 
        with
            | e -> printfn "Exception when writing message(s) to stream: %s" e.Message

    /// Constructs the IRC message to be sent from a Message object
    let messageToString (message: Message) =
        let mutable messageString = ""
        match message.Verb with
        | Some verb -> messageString <- messageString + verb.Value + " "
        | None -> ()
        match message.Params with
        | Some parameters -> 
            parameters.Value
            |> Array.iter (fun p -> messageString <- messageString + p.Value + " ")
        | None -> ()
        messageString <- messageString.TrimEnd(' ') + "\r\n"
        messageString

    /// Construct a single outboud IRC message from a list of messages
    let messagesToString (messages: Message list) =
        let mutable outboundMessage = ""
        messages |> List.iter (fun (m: Message) -> outboundMessage <- outboundMessage + (messageToString m) )
        outboundMessage

    /// Contains an internal async loop that looks at clientData.OutQueue and sends the messages
    /// Has a 10ms sleep duration between each message sent 
    /// will send multiple messages at the same time
    let writeStream (clientData: IRCClientData) (stream: NetworkStream) (writeInterval: int) =
        let sw = System.Diagnostics.Stopwatch()

        let rec writeLoop() =
            async {
                let outboundMessage = 
                    match clientData.OutQueue.Count with
                    | 0 -> ""
                    | 1 -> messageToString  clientData.OutQueue.PopMessage
                    | _ -> messagesToString clientData.OutQueue.PopMessages

                if outboundMessage <> "" then
                    sendIrcMessage stream outboundMessage

                Thread.Sleep (writeInterval)

                match clientData.TokenSource.IsCancellationRequested with
                | false -> return! writeLoop()
                | true -> ()
            }

        writeLoop()