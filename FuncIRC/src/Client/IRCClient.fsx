#load "../Networking/TCPClient.fsx"
#load "../Networking/IRCStreamWriter.fsx"
#load "../Networking/ExternalIPAddress.fsx"
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
open ExternalIPAddress

// TODO: Remove recursive dependency in module
#if !DEBUG
module internal IRCClient =
#else
module IRCClient =
#endif

    /// <summary>
    /// Handles all the information related to the IRC part of the client
    /// </summary>
    type IRCClient (client: TCPClient) as this =
        // # MUTABLES
        let mutable userInfoSelf:   IRCUserInfo        = default_IRCUserInfo
        let mutable serverInfo:     IRCServerInfo      = default_IRCServerInfo
        let mutable serverMOTD:     IRCServerMOTD      = MOTD []
        let mutable serverFeatures: IRCServerFeatures  = Features Map.empty
        let mutable serverChannels: IRCServerChannels  = {Channels = Map.empty}

        let mutable registeredWithServer: bool = false

        // # FIELDS
        /// CancellationTokenSource for the internal tasks
        let tokenSource: CancellationTokenSource = new CancellationTokenSource()
        /// MailboxProcessor to handle outbound messages
        let outQueue: MailboxProcessor<Message> = streamWriter (client)
        /// Concurrent way to update serverInfo using MailboxProcessor
        let serverInfoUpdateQueue: MailboxProcessor<IRCServerInfo> =
            (fun newInfo -> serverInfo <- newInfo) |> mailboxProcessorFactory<IRCServerInfo>

        // # EVENTS
        /// Event when the client was disconnected from server
        let clientDisconnected:   Event<_> = new Event<_>()
        /// Event when wanting to disconnect the client
        let disconnectClient:     Event<_> = new Event<_>()
        /// Event when a new message has been received
        let messageSubscription:  Event<_> = new Event<_>()
        /// Error numeric from server
        let errorNumericReceived: Event<_> = new Event<_>()

        /// Sets the Source part of userInfoSelf with the Nick and User of userInfoSelf
        let prepareUserInfoSelfSource() =
            this.SetUserInfoSelf <|
            match userInfoSelf.Source with
            | Some source -> 
                { userInfoSelf with 
                    Source = Some { source with Nick = Some userInfoSelf.Nick; 
                                                User = Some userInfoSelf.User } }
            | None -> userInfoSelf

        do
            userInfoSelf <- { Nick = ""; User = ""; Source = Some { Nick = None; User = None; Host = Some (getExternalIPAddress()) } }

        #if DEBUG
        new () = new IRCClient (new TCPClient ("", 0, false))
        #endif

        interface IDisposable with
            member this.Dispose() =
                (outQueue :> IDisposable).Dispose()
                (serverInfoUpdateQueue :> IDisposable).Dispose()
                tokenSource.Dispose()

        /// Runs the dispose function on this object cast as IDisposable
        member this.Dispose() = (this :> IDisposable).Dispose()

//#region internal members
        // # FIELDS
        /// CancellationTokenSource for internal tasks
        member internal this.TokenSource = tokenSource
        /// CancellationToken from this.TokenSource
        member internal this.Token = tokenSource.Token
        /// User info of the connected client
        member internal this.SetUserInfoSelf (userInfo: IRCUserInfo) = userInfoSelf <- userInfo
        /// Set after successfully connected with server
        member internal this.SetRegisteredWithServer value = 
            registeredWithServer <- value
            prepareUserInfoSelfSource()
        /// Server info
        member internal this.ServerInfo 
            with get()     = serverInfo
            and set(value) = serverInfoUpdateQueue.Post value
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
        /// Returns the user info of this client
        member this.GetUserInfoSelf: IRCUserInfo = userInfoSelf
        /// Returns the server info of the connected server
        member this.GetServerInfo     = serverInfo
        /// Returns the MOTD of the server if there is any
        member this.GetServerMOTD     = serverMOTD.Value
        /// Returns the server features as a (string * string) Map
        member this.GetServerFeatures = serverFeatures.Value

        /// <summary> Returns the information about a channel if it exists </summary>
        /// <returns> Some of IRCChannelInfo if it exists, None if not </returns>
        member this.GetChannelInfo (channel: string) =
            if serverChannels.Channels.ContainsKey channel then
                Some serverChannels.Channels.[channel]
            else
                None

        /// TODO: Add outboud message validation
        /// Adds one message to the outbound mailbox processor
        member this.AddOutMessage (message: Message) = 
            outQueue.Post {message with Source = userInfoSelf.Source}
            
        /// Adds multiple messages to the outbound mailbox processor
        member this.AddOutMessages (messages: Message list) = messages |> List.iter this.AddOutMessage
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


