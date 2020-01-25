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
    let latin1Encoding = Encoding.GetEncoding("ISO-8859-1") // Latin-1
    let utf8Encoding = Encoding.UTF8

    let parseByteString (data: byte array) =
        try
            utf8Encoding.GetString(data, 0, data.Length)
        with
            e -> latin1Encoding.GetString(data, 0, data.Length)

    let readStream (client: Client) (callback: string * Client -> unit) =
        let data = [| byte 0 |]

        let rec readLoop(received: string) =
            async {
                try
                    let bytes = client.Stream.Read(data, 0, data.Length)

                    let receivedData = parseByteString data

                    let receivedNext = received + receivedData

                    if receivedNext.EndsWith ("\r\n") then
                        callback (received, client)
                        return! readLoop("")
                    else
                        return! readLoop(receivedNext)

                with e -> printfn "Exception: %s" e.Message
            }

        readLoop("")

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
                    | "NOTICE" -> ()
                    | "PRIVMSG" -> ()
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