#load "ConnectionClient.fsx"
#load "MessageSubscription.fsx"
#load "MessageQueue.fsx"
#load "../IRC/MessageTypes.fsx"

namespace FuncIRC

open System.Threading
open ConnectionClient
open MessageSubscription
open MessageQueue
open MessageTypes
open System.Net.Sockets

module IRCClientData =
    type IRCUserInfo =
        {
            Source: Source
        }

    type IRCClientData() =
        // # FIELDS
        let tokenSource: CancellationTokenSource        = new CancellationTokenSource()
        let subscriptionQueue: MessageSubscriptionQueue = MessageSubscriptionQueue()
        let outQueue: MessageQueue                      = MessageQueue()
        let inQueue: MessageQueue                       = MessageQueue()

        // # EVENTS
        let clientDisconnected: Event<_> = new Event<_>()
        let disconnectClient:   Event<_> = new Event<_>()
        let sendMessage:        Event<_> = new Event<_>()

        // # MUTABLES
        let mutable userInfoSelf: IRCUserInfo ValueOption = ValueOption.ValueNone

        // # CONFIG
        let streamWriteInterval        = 10
        let tcpClientKeepAliveInterval = 50
        let cancelAwaitTime            = streamWriteInterval + tcpClientKeepAliveInterval

//#region internal members
        // # CONFIG
        member internal this.StreamWriteInterval        = streamWriteInterval
        member internal this.TcpClientKeepAliveInterval = tcpClientKeepAliveInterval
        member internal this.CancelAwaitTime            = cancelAwaitTime

        // # FIELDS
        member internal this.TokenSource       = tokenSource
        member internal this.Token             = tokenSource.Token
        member internal this.SubscriptionQueue = subscriptionQueue
        member internal this.OutQueue          = outQueue
        member internal this.InQueue           = inQueue

        member internal this.SetUserInfoSelf userInfo = userInfoSelf <- userInfo

        // # EVENTS Triggers
        member internal this.ClientDisconnected() = clientDisconnected.Trigger()
        member internal this.SendMessageTrigger() = sendMessage.Trigger()
        // # Internal EVENTS
        [<CLIEvent>]
        member internal this.DisconnectClientEvent = disconnectClient.Publish
        [<CLIEvent>]
        member internal this.SendMessageEvent      = sendMessage.Publish
//#endregion

//#region external members
        member this.GetUserInfoSelf              = userInfoSelf

        member this.AddSubscription subscription = subscriptionQueue.AddSubscription subscription
        member this.AddOutMessage message        = outQueue.AddMessage message; this.SendMessageTrigger ()
        member this.AddOutMessages messages      = outQueue.AddMessages messages; this.SendMessageTrigger ()
//#endregion

//#region External Events
        [<CLIEvent>]
        member this.ClientDisconnectedEvent = clientDisconnected.Publish

        member this.DisconnectClient() = disconnectClient.Trigger()
//#endregion