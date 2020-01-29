#load "MessageQueue.fsx"
#load "IRCInformation.fsx"
#load "../IRC/MessageTypes.fsx"

namespace FuncIRC

open System
open IRCInformation
open System.Threading
open MessageQueue
open MessageTypes

// TODO: Remove recursive dependency in module
module IRCClientData =
//#region IRCClientData implementation
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
        let mutable userInfoSelf: IRCUserInfo option = None
        let mutable serverInfo: IRCServerInfo = default_IRCServerInfo
        let mutable serverMOTD: IRCServerMOTD = MOTD [""]

//#region private members
        /// Messages from the outbound message queue
        member private this.OutQueue    = outQueue
//#endregion

//#region internal members
        // # FIELDS
        /// CancellationTokenSource for internal tasks
        member internal this.TokenSource = tokenSource
        /// CancellationToken from this.TokenSource
        member internal this.Token       = tokenSource.Token
        /// User info of the connected client
        member internal this.SetUserInfoSelf userInfo = userInfoSelf <- Some userInfo
        /// Server info
        member internal this.ServerInfo 
            with get()     = serverInfo
            and set(value) = serverInfo <- value
        /// Server MOTD
        member internal this.ServerMOTD
            with get() = serverMOTD.Value
            and set(value) = serverMOTD <- MOTD value

        // # EVENTS Triggers
        /// Dispatches the clientDisconnected event
        member internal this.ClientDisconnected() = clientDisconnected.Trigger()
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
        member this.GetServerInfo   = serverInfo
        member this.GetServerMOTD   = serverMOTD.Value

        // TODO: Add outboud message validation
        /// Adds one message to the outbound queue and triggers the sendMessage event
        member this.AddOutMessage message   = outQueue.AddMessage message; sendMessage.Trigger()
        /// Adds multiple messages to the outbound queue and triggers the sendMessage event
        member this.AddOutMessages messages = outQueue.AddMessages messages; sendMessage.Trigger()
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

//#region Internal extension methods to IRCClientData
#if !DEBUG
    type internal IRCClientData with
#else
    type IRCClientData with
#endif
        /// Retreives outbound messages, if any, from the outbound MessageQueue in IRCClientData
        member this.GetOutboundMessages =
            match this.OutQueue with
            | EmptyQueue -> ""
            | SingleItemInQueue item     -> item.ToMessageString
            | MultipleItemsInQueue items -> messagesToString items
//#endregion