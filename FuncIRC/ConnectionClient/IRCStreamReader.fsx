#load "ConnectionClient.fsx"
#load "IRCClientData.fsx"
#load "../IRC/MessageParser.fsx"
#load "../IRC/MessageParserInternals.fsx"
#load "../IRC/MessageTypes.fsx"
#load "../IRC/NumericReplies.fsx"
#load "../IRC/MessageHandlers.fsx"
#load "../Utils/StringHelpers.fsx"

namespace FuncIRC

open ConnectionClient
open System.Diagnostics
open MessageParser
open MessageParserInternals
open MessageTypes
open NumericReplies
open IRCClientData
open System.Net.Sockets
open StringHelpers

module internal IRCStreamReader =
    /// Parses the whole message string from the server and runs the corresponding sub-handlers for tags, source, verb and params
    let receivedDataHandler (data: string, clientData: IRCClientData) =
        data.Trim(' ').Replace("\r\n", "")
        |> function
        | "" -> ()
        | data ->
            //printfn "Message: %s" data
            let message = parseMessageString data

            match message.Verb with // Check if incoming message verb is a numeric
            | Some verb ->
                let verbName = 
                    match verb with
                    | IsVerbName command -> Verb command
                    | IsNumeric numeric -> Verb (numericReplies.[numeric].ToString())

                clientData.MessageSubscriptionTrigger {message with Verb = Some verbName}
            | None -> ()

    /// Responsible for reading the incoming byte stream
    /// Reads on byte at a time, dispatches the callback delegate when \r\n EOM marker is found
    /// TODO: Make it dependant on the CancellationToken of the client
    let readStream (clientData: IRCClientData) (client: TCPClient) =
        let data = [| byte 0 |]

        let rec readLoop(received: string) =
            async {
                try
                    client.ReadFromStream data 0 data.Length |> ignore

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