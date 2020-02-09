#load "TCPClient.fsx"
#load "../IRC/Types/NumericReplies.fsx"
#load "../IRC/Types/MessageTypes.fsx"

namespace FuncIRC

open TCPClient
open MessageTypes
open System.Text
open System.Threading
open System.Net.Sockets

#if !DEBUG
module internal IRCStreamWriter =
#else
module IRCStreamWriter =
#endif
    /// Verifies and sends the message string to the client stream
    /// Preferred syntax for sending messages is client |> sendIrcMessage <| message
    let sendIrcMessage (client: TCPClient) (message: string) =
        let messageData =
            match message with
            | message when message.EndsWith("\r\n") -> Encoding.UTF8.GetBytes (message)
            | _ -> Encoding.UTF8.GetBytes (message + "\r\n")

        try
            client.WriteToStream messageData
            printfn "Sending Message(s):\n\t%s" (message.Replace("\r\n", "\n\t"))
        with
            | e -> printfn "Exception when writing message(s) to stream: %s" e.Message

    /// Creates a MailboxProcessor resposible for sending messages using the Stream of the TcpClient
    let streamWriter (client: TCPClient) =
        MailboxProcessor<Message>.Start 
            (fun outbox ->
                let rec loop() = async {
                    let! msg = outbox.Receive()
                    
                    client |> sendIrcMessage <| msg.ToMessageString
                    return! loop()
                }

                loop()
            )