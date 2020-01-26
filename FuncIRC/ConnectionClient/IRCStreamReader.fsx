#load "ConnectionClient.fsx"
#load "IRCStreamWriter.fsx"
#load "MessageSubscription.fsx"
#load "../IRC/MessageParser.fsx"
#load "../IRC/MessageTypes.fsx"
#load "../IRC/NumericReplies.fsx"
#load "../IRC/VerbHandlers.fsx"

namespace FuncIRC

open System.Text
open ConnectionClient
open IRCStreamWriter
open MessageParser
open MessageTypes
open NumericReplies
open VerbHandlers
open MessageSubscription
open System.Threading

module IRCStreamReader =
    exception IncomingByteMessageParsingException

    let latin1Encoding = Encoding.GetEncoding("ISO-8859-1") // Latin-1
    let utf8Encoding = Encoding.UTF8

    /// Takes a byte array and first attemps to decode using UTF8, uses Latin-1 if that fails
    let parseByteString (data: byte array) =
        try
            utf8Encoding.GetString(data, 0, data.Length)
        with
            e ->
                try 
                    latin1Encoding.GetString(data, 0, data.Length)
                with
                | e -> raise IncomingByteMessageParsingException

    /// Responsible for reading the incoming byte stream
    /// Reads on byte at a time, dispatches the callback delegate when \r\n EOM marker is found
    /// TODO: Make it dependant on the CancellationToken of the client
    let readStream (client: TCPClient) (callback: string * TCPClient * MessageSubscriptionQueue -> unit) (messageSubQueue: MessageSubscriptionQueue) =
        let data = [| byte 0 |]

        let rec readLoop(received: string) =
            async {
                try
                    client.Stream.Read(data, 0, data.Length) |> ignore

                    let receivedData = parseByteString data
                    let receivedNext = received + receivedData

                    if receivedNext.EndsWith ("\r\n") then
                        callback (received, client, messageSubQueue)
                        return! readLoop("")
                    else
                        return! readLoop(receivedNext)

                with e -> printfn "Exception: %s" e.Message
            }

        readLoop("")

    let runMessageSubscriptionCallbacks (messageSubQueue: MessageSubscriptionQueue) (verb: Verb) (callbackParams: TCPClient * Message) =
        messageSubQueue.GetSubscriptions verb
        |> Array.iter 
            (fun x -> 
                x.Callback callbackParams
                if not x.Continuous then
                    messageSubQueue.RemoveSubscription x
            )

    /// Parses the whole message string from the server and runs the corresponding sub-handlers for tags, source, verb and params
    let receivedDataHandler (data: string, client: TCPClient, messageSubQueue: MessageSubscriptionQueue) =
        data.Trim(' ').Replace("\r\n", "")
        |> function
        | "" -> ()
        | data ->
            //printfn "Message: %s" data
            let message = parseMessageString data
            
            match message.Verb with
            | Some verb ->
                let verbName = 
                    match verb with
                    | IsVerbName command -> Verb command
                    | IsNumeric numeric -> Verb (numericReplies.[numeric].ToString())
                runMessageSubscriptionCallbacks messageSubQueue verbName (client, message)
            | None -> ()