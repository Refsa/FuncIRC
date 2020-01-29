#load "MessageQueue.fsx"
#load "../IRC/MessageTypes.fsx"

namespace FuncIRC

open System
open System.Threading
open MessageQueue
open MessageTypes

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
        /// CancellationTokenSource for the internal tasks
        let tokenSource: CancellationTokenSource = new CancellationTokenSource()
        /// Contains all the outbound messages since the last sendMessage event trigger
        let outQueue: MessageQueue = MessageQueue()

        // # EVENTS
        /// Event when the client was disconnected from server
        let clientDisconnected:  Event<_> = new Event<_>()
        /// Event when wanting to disconnect the client
        let disconnectClient:    Event<_> = new Event<_>()
        /// Event when a new message has been received
        let messageSubscription: Event<_> = new Event<_>()
        /// Event when wanting to send messages
        let sendMessage:         Event<_> = new Event<_>()

        // # MUTABLES
        let mutable userInfoSelf: IRCUserInfo ValueOption = ValueOption.ValueNone

//#region internal members
        // # FIELDS
        /// CancellationTokenSource for internal tasks
        member internal this.TokenSource       = tokenSource
        /// CancellationToken from this.TokenSource
        member internal this.Token             = tokenSource.Token
        /// Messages from the outbound message queue
        member internal this.OutQueue          = outQueue

        member internal this.SetUserInfoSelf userInfo = userInfoSelf <- userInfo

        // # EVENTS Triggers
        /// Dispatches the clientDisconnected event
        member internal this.ClientDisconnected() = clientDisconnected.Trigger()
        /// Dispatches the sendMessage event
        member internal this.SendMessageTrigger() = sendMessage.Trigger()
        /// Dispatches the messageSubscription event
        member internal this.MessageSubscriptionTrigger(message: Message) = messageSubscription.Trigger (message, this)

        // # Internal EVENTS
        /// Event binder for disconnectClient event
        [<CLIEvent>]
        member internal this.DisconnectClientEvent = disconnectClient.Publish
        /// Event binder for sendMessage event
        [<CLIEvent>]
        member internal this.SendMessageEvent = sendMessage.Publish
//#endregion

//#region external members
        member this.GetUserInfoSelf = userInfoSelf

        // TODO: Add outboud message validation
        /// Adds one message to the outbound queue and triggers the sendMessage event
        member this.AddOutMessage message   = outQueue.AddMessage message; this.SendMessageTrigger ()
        /// Adds multiple messages to the outbound queue and triggers the sendMessage event
        member this.AddOutMessages messages = outQueue.AddMessages messages; this.SendMessageTrigger ()
//#endregion

//#region External Events
        /// Event binder for the clientDisconnected event
        [<CLIEvent>]
        member this.ClientDisconnectedEvent = clientDisconnected.Publish
        /// Event binder for the messageSubscription event
        [<CLIEvent>]
        member this.MessageSubscriptionEvent = messageSubscription.Publish

        /// Event trigger for the disconnectClient event
        member this.DisconnectClient() = disconnectClient.Trigger()
//#endregion