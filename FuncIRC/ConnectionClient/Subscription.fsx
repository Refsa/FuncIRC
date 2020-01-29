#load "../IRC/MessageTypes.fsx"
#load "../IRC/NumericReplies.fsx"

namespace FuncIRC

open System
open MessageTypes
open NumericReplies

module Subscription =
    (*type ResponseType =
        | Error
        | NotImplemented
        | Subscription
        | Message
        | ClientInfoSelf
        | ClientInfoOther
        | ClientInfoChanged*)

    type SubscriptionResponse =
        {
            Verb: string
            Content: string
        } //with

        //static member NewError content = {ResponseType = ResponseType.Error; Content = content}
        //static member NotImplemented content = {ResponseType = ResponseType.NotImplemented; Content = content}
        //static member NewSubscription content = {ResponseType = ResponseType.Subscription; Content = content}
        //static member NewMessage content = {ResponseType = ResponseType.Message; Content = content}

    type Subscription<'M, 'T> =
        {
            Timestamp: DateTime
            Verb: Verb
            #if DEBUG
            Callback: 'M * 'T -> SubscriptionResponse option
            #else
            Callback: 'M * 'T -> unit
            #endif
            Continuous: bool
            AdditionalData: string option
        } with
        
        static member NewRepeat verb callback = 
            { Timestamp = DateTime.UtcNow; Verb = verb; Callback = callback; Continuous = true; AdditionalData = None }

        static member NewSingle verb callback = 
            { Timestamp = DateTime.UtcNow; Verb = verb; Callback = callback; Continuous = false; AdditionalData = None }

        static member Equal x y = 
            x.Timestamp = y.Timestamp && x.Verb = y.Verb && x.Continuous = y.Continuous