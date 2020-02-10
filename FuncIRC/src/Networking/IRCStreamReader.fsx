#load "TCPClient.fsx"
#load "IRCClient.fsx"
#load "../IRC/MessageParser.fsx"
#load "../IRC/MessageParserInternals.fsx"
#load "../IRC/MessageTypes.fsx"
#load "../IRC/NumericReplies.fsx"
#load "../IRC/MessageHandlers.fsx"
#load "../Utils/StringHelpers.fsx"
#load "../Utils/MailboxProcessorHelpers.fsx"

namespace FuncIRC

open System
open System.Diagnostics

open IRCClient
open MailboxProcessorHelpers
open MessageParser
open MessageParserInternals
open MessageTypes
open NumericReplies
open StringHelpers
open TCPClient

#if !DEBUG
module internal IRCStreamReader =
#else
module IRCStreamReader =
#endif
    /// Parses the whole message string from the server and runs the corresponding sub-handlers for tags, source, verb and params
    let receivedDataHandler (data: string, clientData: IRCClient) =
        data.Trim(' ').Replace("\r\n", "")
        |> function
        | "" -> ()
        | data ->
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
    let readStream (clientData: IRCClient) (client: TCPClient) =
        let processorAgent =
            ( fun msg -> receivedDataHandler(msg, clientData) ) |> mailboxProcessorFactory<string>
            
        let data = [| byte 0 |]
        let rec readLoop(received: string) =
            async {
                try
                    client.ReadFromStream data 0 data.Length |> ignore

                    let receivedData = parseByteString data
                    let receivedNext = received + receivedData

                    match clientData.TokenSource.IsCancellationRequested with
                    | true -> 
                        (processorAgent :> IDisposable).Dispose()
                    | false ->
                        if receivedNext.EndsWith ("\r\n") then
                            processorAgent.Post receivedNext

                            return! readLoop("")
                        else
                            return! readLoop(receivedNext)
                with
                    e -> printfn "readLoop - Exception: %s - Content of received so far: %s" e.Message received
            }

        readLoop("")