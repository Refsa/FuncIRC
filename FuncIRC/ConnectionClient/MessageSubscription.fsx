#load "ConnectionClient.fsx"
#load "../IRC/MessageTypes.fsx"

namespace FuncIRC

open System
open MessageTypes
open ConnectionClient

module MessageSubscription =
    type ResponseType =
        | Error
        | NotImplemented
        | Subscription
        | Message

    type MessageResponse =
        {
            ResponseType: ResponseType
            Content: string
        } with

        static member NewError content = {ResponseType = ResponseType.Error; Content = content}
        static member NotImplemented content = {ResponseType = ResponseType.NotImplemented; Content = content}
        static member NewSubscription content = {ResponseType = ResponseType.Subscription; Content = content}
        static member NewMessage content = {ResponseType = ResponseType.Message; Content = content}

    type MessageSubscription =
        {
            Timestamp: DateTime
            Verb: Verb
            Callback: Message -> MessageResponse option
            Continuous: bool
            AdditionalData: string option
        } with
        
        static member NewRepeat verb callback = 
            { Timestamp = DateTime.UtcNow; Verb = verb; Callback = callback; Continuous = true; AdditionalData = None }

        static member NewSingle verb callback = 
            { Timestamp = DateTime.UtcNow; Verb = verb; Callback = callback; Continuous = false; AdditionalData = None }

        static member Equal x y = 
            x.Timestamp = y.Timestamp && x.Verb = y.Verb && x.Continuous = y.Continuous

    [<Sealed>]
    type MessageSubscriptionQueue() =
        let mutable subscriptions: MessageSubscription array = Array.empty

        member this.Count 
            with get() = subscriptions.Length

        /// TODO: There is going to be a race condition here, add a semaphore to protect subscriptions array
        member this.AddSubscription (messageSubscription: MessageSubscription) =
            subscriptions <- (Array.append subscriptions [| messageSubscription |])
            Array.sortInPlaceBy (fun x -> x.Verb.Value) subscriptions

        member this.GetSubscriptions (verb: Verb): MessageSubscription array =
            Array.FindAll (subscriptions, (fun ms -> ms.Verb = verb))

        member this.RemoveSubscription (messageSubscription: MessageSubscription) =
            subscriptions <- subscriptions |> Array.filter (((MessageSubscription.Equal)messageSubscription) >> not)