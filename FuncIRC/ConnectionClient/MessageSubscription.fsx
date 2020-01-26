#load "ConnectionClient.fsx"
#load "../IRC/MessageTypes.fsx"
#load "../IRC/MessageHandlers.fsx"
#load "../IRC/NumericReplies.fsx"

namespace FuncIRC

open System
open MessageTypes
open ConnectionClient
open MessageHandlers
open NumericReplies

module MessageSubscription =
    type MessageSubscription =
        {
            Timestamp: DateTime
            Verb: Verb
            Callback: TCPClient * Message -> unit
            Continuous: bool
        } with
        
        static member NewRepeat verb callback = 
            { Timestamp = DateTime.UtcNow; Verb = verb; Callback = callback; Continuous = true }

        static member NewSingle verb callback = 
            { Timestamp = DateTime.UtcNow; Verb = verb; Callback = callback; Continuous = false }

        static member Equal x y = 
            x.Timestamp = y.Timestamp && x.Verb = y.Verb && x.Continuous = y.Continuous

    type MessageSubscriptionQueue() =
        let mutable subscriptions: MessageSubscription array = Array.empty

        member this.Count 
            with get() = subscriptions.Length

        member this.AddSubscription (messageSubscription: MessageSubscription) =
            subscriptions <- (Array.append subscriptions [| messageSubscription |])
            Array.sortInPlaceBy (fun x -> x.Verb.Value) subscriptions

        member this.GetSubscriptions (verb: Verb): MessageSubscription array =
            Array.FindAll (subscriptions, (fun ms -> ms.Verb = verb))

        member this.RemoveSubscription (messageSubscription: MessageSubscription) =
            //subscriptions <- subscriptions.[0..index - 1] |> Array.append <| subscriptions.[index + 1..subscriptions.Length - 1]
            subscriptions <- subscriptions |> Array.filter (((MessageSubscription.Equal)messageSubscription) >> not)

    let setupRequiredIrcMessageSubscriptions (messageSubQueue: MessageSubscriptionQueue) =
        messageSubQueue.AddSubscription (MessageSubscription.NewRepeat (Verb ("PING")) pongMessageHandler)

        messageSubQueue.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_WELCOME.Value)) rplWelcomeHandler)
