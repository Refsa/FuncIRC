#load "ConnectionClient.fsx"
#load "IRCStreamWriter.fsx"
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

module IRCStreamReader =

    let readStream (client: Client) (callback: string * Client -> unit) =
        let mutable received = ""

        let rec readLoop() =
            async {
                try
                    let data =
                        [| byte 0 |]

                    let bytes = client.Stream.Read(data, 0, data.Length)

                    let receivedData = Encoding.UTF8.GetString(data, 0, bytes)
                    received <- received + receivedData

                    if received.EndsWith ("\r\n") then
                        callback (received, client)
                        received <- ""

                    return! readLoop()
                with e -> printfn "Exception: %s" e.Message
            }

        readLoop()

    let receivedDataHandler (data: string, client: Client) =
        let data = data.Trim(' ').Replace("\r\n", "")
        match data with
        | "" -> ()
        | _ ->
            printfn "Message: %s" data 
            let message = parseMessageString data
            if message.Verb.IsSome then
                let verb = message.Verb.Value.Value
                printf "\tVerb: %s - " (verb)

                let numeric = verbToInt message.Verb.Value
                match numeric with
                | None -> 
                    match verb with
                    | "PING" -> 
                        let response = verbHandlers.[NumericsReplies.PING]()
                        printf "Response: %s\n" response
                        client |> sendIrcMessage <| response
                    | _ -> printf "Handler for Verb [%s] does not exist" verb
                | Some numeric ->
                    let numericName = numericReplies.[numeric]
                    let handler = verbHandlers.TryFind numericName
                    match handler with
                    | Some handler -> 
                        let response = verbHandlers.[numericName]()
                        printf "Response: %s\n" response
                    | None ->
                        printf "Handler for Numeric(%d): %s does not exist\n" numeric (numericName.ToString())