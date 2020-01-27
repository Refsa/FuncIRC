#load "ConnectionClient.fsx"
#load "IRCClientData.fsx"
#load "MessageSubscription.fsx"
#load "../IRC/MessageParser.fsx"
#load "../IRC/MessageTypes.fsx"
#load "../IRC/NumericReplies.fsx"
#load "../IRC/VerbHandlers.fsx"
#load "../Utils/StringHelpers.fsx"

namespace FuncIRC

open System.Text
open System.Diagnostics
open MessageParser
open MessageTypes
open NumericReplies
open VerbHandlers
open MessageSubscription
open IRCClientData
open System.Net.Sockets
open StringHelpers

module internal IRCStreamReader =
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

    let private sw = Stopwatch()
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
    let readStream (clientData: IRCClientData) (stream: NetworkStream) =
        let data = [| byte 0 |]

        let rec readLoop(received: string) =
            async {
                try
                    stream.Read(data, 0, data.Length) |> ignore

                    let receivedData = parseByteString data
                    let receivedNext = received + receivedData

                    match clientData.TokenSource.IsCancellationRequested with
                    | true -> ()
                    | false ->
                        if receivedNext.EndsWith ("\r\n") then
                            receivedDataHandler (receivedNext, clientData)
                            return! readLoop("")
                        else
                            return! readLoop(receivedNext)
                with 
                    e -> printfn "readLoop - Exception: %s" e.Message
            }

        readLoop("")