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
                    client.Stream.Read(data, 0, data.Length) |> ignore

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

    let handleVerb (verb: Verb) =
        printf "\tVerb: %s - " (verb.Value)

        let handler =
            match verb with
            | IsVerbName command ->
                match command with
                | IsPing handler -> handler
                | IsNotice handler -> handler
                | IsPrivMsg handler -> handler
                | UnknownVerbName handler -> printf "Handler for Verb [%s] does not exist" command; handler
            | IsNumeric numeric ->
                let numericName = numericReplies.[numeric]
                let handler = verbHandlers.TryFind numericName
                match handler with
                | Some handler -> handler
                | None ->
                    printf "Handler for Numeric(%d): %s does not exist\n" numeric (numericName.ToString())
                    noCallback
        
        handler()

    let receivedDataHandler (data: string, client: Client) =
        let data = data.Trim(' ').Replace("\r\n", "")
        match data with
        | "" -> ()
        | _ ->
            printfn "Message: %s" data 
            let message = parseMessageString data
            
            if message.Verb.IsSome then
                message.Verb.Value |> handleVerb
                |> function
                | "" -> ()
                | response ->
                    printf "Response: %s\n" response
                    client |> sendIrcMessage <| response