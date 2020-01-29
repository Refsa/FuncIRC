#load "MessageSubscription.fsx"
#load "../IRC/MessageTypes.fsx"

namespace FuncIRC

open System
open MessageSubscription
open MessageTypes

module MessageSubscriptionQueue =
    type SubscriptionQueue<'M>() =
        let mutable subscriptions: 'M array = Array.empty

        member this.Count 
            with get() = subscriptions.Length

        /// TODO: There is going to be a race condition here, add a semaphore to protect subscriptions array
        member this.AddSubscription (subscription: 'M) =
            subscriptions <- (Array.append subscriptions [| subscription |])

        member this.GetSubscriptions (verb: Verb) (equalDelegate: 'M * Verb -> bool): 'M array =
            Array.FindAll (subscriptions, (fun ms -> equalDelegate (ms, verb)))

        member this.RemoveSubscription (subscription: 'M) (equalDelegate: 'M * 'M -> bool) =
            subscriptions <- subscriptions |> Array.filter (fun m -> not (equalDelegate(m, subscription)))