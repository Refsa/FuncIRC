#load "ConnectionClient.fsx"
#load "IRCClient.fsx"
#load "MessageSubscription.fsx"
#load "../IRC/MessageParser.fsx"
#load "../IRC/MessageTypes.fsx"
#load "../IRC/NumericReplies.fsx"
#load "../IRC/VerbHandlers.fsx"

namespace FuncIRC

open System.Text
open MessageParser
open MessageTypes
open NumericReplies
open VerbHandlers
open MessageSubscription
open IRCClientData

module IRCStreamReader =
    /// Raised when the incoming byte array/stream couldn't be parsed with either UTF-8 or Latin-1
    exception IncomingByteMessageParsingException

    let latin1Encoding = Encoding.GetEncoding("ISO-8859-1") // Latin-1
    let utf8Encoding = Encoding.UTF8 // UTF-8

    /// Takes a byte array and first attemps to decode using UTF8, uses Latin-1 if that fails
    /// raises: <typeref "IncomingByteMessageParsingException">
    let parseByteString (data: byte array) =
        try
            utf8Encoding.GetString(data, 0, data.Length)
        with
            e ->
                try 
                    latin1Encoding.GetString(data, 0, data.Length)
                with
                | e -> raise IncomingByteMessageParsingException

    ///
    let runMessageSubscriptionCallbacks (clientData: IRCClientData) (verb: Verb) (callbackParams: Message) =
        clientData.SubscriptionQueue.GetSubscriptions verb
        |> Array.iter 
            (fun x -> 
                x.Callback callbackParams
                |> function
                | Some response ->
                    match response.ResponseType with
                    | ResponseType.Message -> ()
                    | _ -> ()
                | None -> ()

                if not x.Continuous then
                    clientData.SubscriptionQueue.RemoveSubscription x
            )

    /// Parses the whole message string from the server and runs the corresponding sub-handlers for tags, source, verb and params
    let receivedDataHandler (data: string, clientData: IRCClientData) =
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

                runMessageSubscriptionCallbacks clientData verbName message
            | None -> ()

    /// Responsible for reading the incoming byte stream
    /// Reads on byte at a time, dispatches the callback delegate when \r\n EOM marker is found
    /// TODO: Make it dependant on the CancellationToken of the client
    let readStream (clientData: IRCClientData) =
        let data = [| byte 0 |]

        let rec readLoop(received: string) =
            async {
                try 
                    clientData.Client.Stream.Read(data, 0, data.Length) |> ignore

                    let receivedData = parseByteString data
                    let receivedNext = received + receivedData

                    if receivedNext.EndsWith ("\r\n") then
                        receivedDataHandler (receivedNext, clientData)
                        return! readLoop("")
                    else
                        return! readLoop(receivedNext)
                with 
                    e -> printfn "readLoop - Exception: %s" e.Message
            }

        readLoop("")