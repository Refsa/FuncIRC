#load "../Networking/TCPClient.fsx"
#load "../Networking/IRCStreamWriter.fsx"
#load "../Utils/MailboxProcessorHelpers.fsx"
#load "../IRC/Types/IRCInformation.fsx"
#load "../IRC/Types/MessageTypes.fsx"

namespace FuncIRC

open System
open System.Threading

open IRCInformation
open IRCStreamWriter
open MailboxProcessorHelpers
open MessageTypes
open TCPClient

// TODO: Remove recursive dependency in module
#if !DEBUG
module internal IRCClient =
#else
module IRCClient =
#endif

    type IRCClient (client: TCPClient) =
        // # FIELDS
        /// CancellationTokenSource for the internal tasks
        let tokenSource: CancellationTokenSource = new CancellationTokenSource()
        /// MailboxProcessor to handle outbound messages
        let outQueue: MailboxProcessor<Message> = streamWriter (client)

        // # EVENTS
        /// Event when the client was disconnected from server
        let clientDisconnected:   Event<_> = new Event<_>()
        /// Event when wanting to disconnect the client
        let disconnectClient:     Event<_> = new Event<_>()
        /// Event when a new message has been received
        let messageSubscription:  Event<_> = new Event<_>()
        /// Error numeric from server
        let errorNumericReceived: Event<_> = new Event<_>()

        // # MUTABLES
        let mutable userInfoSelf:   IRCUserInfo option = None
        let mutable serverInfo:     IRCServerInfo      = default_IRCServerInfo
        let mutable serverMOTD:     IRCServerMOTD      = MOTD []
        let mutable serverFeatures: IRCServerFeatures  = Features Map.empty
        let mutable serverChannels: IRCServerChannels  = {Channels = Map.empty}

        /// Concurrent way to update serverInfo
        let serverFeaturesUpdateQueue: MailboxProcessor<IRCServerInfo> =
            mailboxProcessorFactory<IRCServerInfo>
                (fun newInfo -> serverInfo <- newInfo)

        #if DEBUG
        new () = new IRCClient (new TCPClient ("", 0, false))
        #endif

        interface IDisposable with
            member this.Dispose() =
                (outQueue :> IDisposable).Dispose()
                (serverFeaturesUpdateQueue :> IDisposable).Dispose()
                tokenSource.Dispose()
                
        member this.Dispose() = (this :> IDisposable).Dispose()

//#region internal members
        // # FIELDS
        /// CancellationTokenSource for internal tasks
        member internal this.TokenSource = tokenSource
        /// CancellationToken from this.TokenSource
        member internal this.Token = tokenSource.Token
        /// User info of the connected client
        member internal this.SetUserInfoSelf userInfo = userInfoSelf <- Some userInfo
        /// Server info
        member internal this.ServerInfo 
            with get()     = serverInfo
            and set(value) = serverFeaturesUpdateQueue.Post value
        /// Server MOTD
        member internal this.ServerMOTD
            with get()     = serverMOTD.Value
            and set(value) = serverMOTD <- MOTD value
        /// Server Features
        member internal this.ServerFeatures
            with get()     = serverFeatures.Value
            and set(value) = serverFeatures <- Features value
        /// Server Channels
        member internal this.SetChannelInfo channel info =
            if serverChannels.Channels.ContainsKey channel then
                serverChannels.Channels <- serverChannels.Channels |> Map.remove channel |> Map.add channel info
            else
                serverChannels.Channels <- serverChannels.Channels |> Map.add channel info

        // # EVENTS Triggers
        /// Dispatches the clientDisconnected event
        member internal this.ClientDisconnected() = clientDisconnected.Trigger()
        /// Dispatches the messageSubscription event
        member internal this.MessageSubscriptionTrigger(message: Message) = messageSubscription.Trigger (message, this)
        /// Dispatches the errorNumericReceived event
        member internal this.ErrorNumericReceivedTrigger (error: string) = errorNumericReceived.Trigger (error)

        // # Internal EVENTS
        /// Event binder for disconnectClient event
        [<CLIEvent>]
        member internal this.DisconnectClientEvent = disconnectClient.Publish
//#endregion internal members

//#region external members
        member this.GetUserInfoSelf   = userInfoSelf
        member this.GetServerInfo     = serverInfo
        member this.GetServerMOTD     = serverMOTD.Value
        member this.GetServerFeatures = serverFeatures.Value

        member this.GetChannelInfo channel =
            if serverChannels.Channels.ContainsKey channel then
                Some serverChannels.Channels.[channel]
            else
                None

        // TODO: Add outboud message validation
        /// Adds one message to the outbound mailbox processor
        member this.AddOutMessage message   = outQueue.Post message
        /// Adds multiple messages to the outbound mailbox processor
        member this.AddOutMessages messages = messages |> List.iter this.AddOutMessage
//#endregion external members

//#region External Events
        /// Event binder for the clientDisconnected event
        [<CLIEvent>]
        member this.ClientDisconnectedEvent = clientDisconnected.Publish
        /// Event binder for the messageSubscription event
        [<CLIEvent>]
        member this.MessageSubscriptionEvent = messageSubscription.Publish
        /// Event binder for the errorNumericReceived event
        [<CLIEvent>]
        member this.ErrorNumericReceivedEvent = errorNumericReceived.Publish

        /// Event trigger for the disconnectClient event
        member this.DisconnectClient() = disconnectClient.Trigger()
//#endregion external events


