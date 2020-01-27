#load "MessageSubscription.fsx"
#load "IRCClientData.fsx"
#load "../IRC/NumericReplies.fsx"
#load "../IRC/MessageTypes.fsx"
#load "../IRC/MessageHandlers.fsx"

namespace FuncIRC

open IRCClientData
open MessageSubscription
open MessageTypes
open NumericReplies
open MessageHandlers
open System.Text
open System.Threading

module IRCStreamWriter =
    /// Exception thrown when parameters to a registration message was missing
    exception RegistrationContentException

    /// Verifies and sends the message string to the client stream
    /// Preferred syntax for sending messages is client |> sendIrcMessage <| message
    let sendIrcMessage (clientData: IRCClientData) (message: string) =
        let messageData =
            match message with
            | message when message.EndsWith("\r\n") -> Encoding.UTF8.GetBytes (message)
            | _ -> Encoding.UTF8.GetBytes (message + "\r\n")

        try
            printfn "Message: %s" message
            clientData.Client.Stream.Write (messageData, 0, messageData.Length) 
        with
            | e -> printfn "Exception when writing message(s) to stream: %s" e.Message

    /// Constructs the IRC message to be sent from a Message object
    let messageToString (message: Message) =
        let mutable messageString = ""
        match message.Verb with
        | Some verb -> messageString <- messageString + verb.Value + " "
        | None -> ()
        match message.Params with
        | Some parameters -> 
            parameters.Value
            |> Array.iter (fun p -> messageString <- messageString + p.Value + " ")
        | None -> ()
        messageString <- messageString.TrimEnd(' ') + "\r\n"
        messageString

    /// Construct a single outboud IRC message from a list of messages
    let messagesToString (messages: Message list) =
        let mutable outboundMessage = ""
        messages |> List.iter (fun (m: Message) -> outboundMessage <- outboundMessage + (messageToString m) )
        outboundMessage

    ///
    let writeStream (clientData: IRCClientData) =
        let rec writeLoop() =
            async {
                if clientData.OutQueue.Count = 0 then
                    Thread.Sleep (10)
                    return! writeLoop()

                let outboundMessage = 
                    match clientData.OutQueue.Count with
                    | 1 -> messageToString clientData.OutQueue.PopMessage
                    | _ -> messagesToString clientData.OutQueue.PopMessages

                sendIrcMessage clientData outboundMessage

                return! writeLoop()
            }

        writeLoop()

    /// Creates a registration message and sends it to the client
    let sendRegistrationMessage (clientData: IRCClientData) (nick: string, user: string, realName: string, pass: string) =
        let messages = 
            match () with
            | _ when pass <> "" && nick <> "" && user <> "" -> 
                [
                    { Tags = None; Source = None; Verb = Some (Verb "CAP"); Params = Some (toParameters [|"LS"; "302"|]) }
                    { Tags = None; Source = None; Verb = Some (Verb "PASS"); Params = Some (toParameters [|pass|]) }
                    { Tags = None; Source = None; Verb = Some (Verb "NICK"); Params = Some (toParameters [|nick|]) }
                    { Tags = None; Source = None; Verb = Some (Verb "USER"); Params = Some (toParameters [|user; "0"; "*"; realName|]) }
                ]
            | _ when nick <> "" && user <> "" -> 
                [
                    { Tags = None; Source = None; Verb = Some (Verb "CAP"); Params = Some (toParameters [|"LS"; "302"|]) }
                    { Tags = None; Source = None; Verb = Some (Verb "NICK"); Params = Some (toParameters [|nick|]) }
                    { Tags = None; Source = None; Verb = Some (Verb "USER"); Params = Some (toParameters [|user; "0"; "*"; realName|]) }
                ]
            | _ -> raise RegistrationContentException

        clientData.OutQueue.AddMessages messages

        clientData.SubscriptionQueue.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_WELCOME.Value)) rplWelcomeHandler)
        clientData.SubscriptionQueue.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_YOURHOST.Value)) rplYourHostHandler)
        clientData.SubscriptionQueue.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_CREATED.Value)) rplCreatedHandler)
        clientData.SubscriptionQueue.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_MYINFO.Value)) rplMyInfoHandler)

    ///
    let sendQuitMessage (clientData: IRCClientData) (message: string) =
        { Tags = None; Source = None; Verb = Some (Verb "QUIT"); Params = Some (toParameters [|message|]) }
        |> clientData.OutQueue.AddMessage