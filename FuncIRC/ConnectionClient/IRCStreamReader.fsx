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

    let readStream (client: Client) (callback: string * Client * (Client * Message -> unit) -> unit) (messageCallback: Client * Message -> unit) =
        let data = [| byte 0 |]

        let rec readLoop(received: string) =
            async {
                try
                    client.Stream.Read(data, 0, data.Length) |> ignore

                    let receivedData = parseByteString data
                    let receivedNext = received + receivedData

                    if receivedNext.EndsWith ("\r\n") then
                        callback (received, client, messageCallback)
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
                | IsPing handler          -> handler
                | IsNotice handler        -> handler
                | IsPrivMsg handler       -> handler
                | UnknownVerbName handler -> handler
            | IsNumeric numeric ->
                let numericName = numericReplies.[numeric]
                let handler = verbHandlers.TryFind numericName
                match handler with
                | Some handler -> handler
                | None         -> (fun () -> {noCallback with Content = numericName.ToString()})
        
        handler()

    let receivedDataHandler (data: string, client: Client, messageCallback: Client * Message -> unit) =
        data.Trim(' ').Replace("\r\n", "")
        |> function
        | "" -> ()
        | data ->
            printfn "Message: %s" data
            let message = parseMessageString data
            
            if message.Verb.IsSome then
                message.Verb.Value |> handleVerb
                |> function
                | handler when handler.Type = VerbHandlerType.NotImplemented ->
                    (if handler.Content <> "" then handler.Content else message.Verb.Value.Value)
                    |> printfn "WARNING: Verb Handler for %s is not implemented"
                | handler when handler.Type = VerbHandlerType.Callback ->
                    messageCallback (client, message)
                | handler when handler.Type = VerbHandlerType.Response ->
                    printf "Response: %s\n" handler.Content
                    client |> sendIrcMessage <| handler.Content
                | handler -> printfn "ERROR: No registered fallback for VerbHandlerType: %s" (handler.Type.ToString())