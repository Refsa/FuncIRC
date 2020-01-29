#load "SubscriptionQueue.fsx"
#load "Subscription.fsx"
#load "MessageQueue.fsx"
#load "../IRC/MessageTypes.fsx"

namespace FuncIRC

open System
open System.Threading
open SubscriptionQueue
open MessageQueue
open MessageTypes
open Subscription

// TODO: Remove recursive dependency in module
module IRCClientData =
    type IRCUserInfo =
        {
            Source: Source
        }

    type IRCServerInfo =
        {
            Name: string
            GlobalUserCount: int
            LocalUserCount: int
        }

    type IRCChannelInfo =  
        {
            Name: string
            UserCount: int
        }

    type IRCClientData() =
        // # FIELDS
        let tokenSource:       CancellationTokenSource  = new CancellationTokenSource()
        let outQueue:          MessageQueue             = MessageQueue()
        let inQueue:           MessageQueue             = MessageQueue()

        // # EVENTS
        let clientDisconnected:  Event<_> = new Event<_>()
        let disconnectClient:    Event<_> = new Event<_>()
        let sendMessage:         Event<_> = new Event<_>()
        let messageSubscription: Event<_> = new Event<_>()

        // # MUTABLES
        let mutable userInfoSelf: IRCUserInfo ValueOption = ValueOption.ValueNone

        // # CONFIG

//#region internal members
        // # CONFIG

        // # FIELDS
        member internal this.TokenSource       = tokenSource
        member internal this.Token             = tokenSource.Token
        member internal this.OutQueue          = outQueue
        member internal this.InQueue           = inQueue

        member internal this.SetUserInfoSelf userInfo = userInfoSelf <- userInfo

        // # EVENTS Triggers
        member internal this.ClientDisconnected() = clientDisconnected.Trigger()
        member internal this.SendMessageTrigger() = sendMessage.Trigger()
        member internal this.MessageSubscriptionTrigger(message: Message) = messageSubscription.Trigger (message, this)

        // # Internal EVENTS
        [<CLIEvent>]
        member internal this.DisconnectClientEvent = disconnectClient.Publish
        [<CLIEvent>]
        member internal this.SendMessageEvent = sendMessage.Publish
//#endregion

//#region external members
        member this.GetUserInfoSelf = userInfoSelf

        // TODO: Add outboud message validation
        member this.AddOutMessage message   = outQueue.AddMessage message; this.SendMessageTrigger ()
        member this.AddOutMessages messages = outQueue.AddMessages messages; this.SendMessageTrigger ()
//#endregion

//#region External Events
        [<CLIEvent>]
        member this.ClientDisconnectedEvent = clientDisconnected.Publish
        [<CLIEvent>]
        member this.MessageSubscriptionEvent = messageSubscription.Publish

        member this.DisconnectClient() = disconnectClient.Trigger()
//#endregion