#load "../../Client/IRCClient.fsx"
#load "../Handlers/ServerFeatureHandlers.fsx"
#load "../Parsers/MessageParserInternals.fsx"
#load "../Types/MessageTypes.fsx"
#load "../Types/NumericReplies.fsx"
#load "../../Utils/RegexHelpers.fsx"

namespace FuncIRC

open MessageTypes
open IRCClient
open IRCInformation
open NumericReplies
open MessageParserInternals
open RegexHelpers
open ServerFeatureHandlers
open System

#if !DEBUG
module internal MessageHandlers =
#else
module MessageHandlers =
#endif

//#region PING message handler
    /// PONG message const 
    let private pongMessage = Message.NewSimpleMessage (Some (Verb "PONG")) None
    /// PING message handler
    let pongMessageHandler (message: Message, clientData: IRCClient) =
        clientData.AddOutMessage pongMessage
//#endregion PING message handler

    /// RPL_WELCOME handler
    let rplWelcomeHandler (message: Message, clientData: IRCClient) =
        ()
        //printfn "RPL_WELCOME: %s" message.Params.Value.Value.[1].Value

    /// RPL_YOURHOST handler
    let rplYourHostHandler (message: Message, clientData: IRCClient) =
        ()
        //printfn "RPL_YOURHOST: %s" message.Params.Value.Value.[1].Value

//#region RPL_CREATED handler
    /// Regex to capture DateTimes in the format of: 23:25:21 Jan 24 2020
    /// TODO: Add capturing of different DateTime formats
    let dateTimeRegex = @"(\d{2}:\d{2}:\d{2}.+)"
    /// RPL_CREATED handler
    let rplCreatedHandler (message: Message, clientData: IRCClient) =
        let wantedParam = message.Params.Value.Value.[1].Value

        let createdDateTimeString =
            matchRegex wantedParam dateTimeRegex
            |> function
            | Some r -> r.[0]
            | None -> ""

        let createdDateTime =
            try
                DateTime.Parse (createdDateTimeString)
            with 
            | _ -> DateTime.MinValue

        clientData.ServerInfo <- {clientData.ServerInfo with Created = createdDateTime}
//#endregion RPL_CREATED handler

    /// RPL_MYINFO handler
    let rplMyInfoHandler (message: Message, clientData: IRCClient) = ()
    /// RPL_LUSERCLIENT handler
    let rplLUserClientHandler (message: Message, clientData: IRCClient) = ()
    /// RPL_LUSERUNKNOWN handler
    let rplLUserUnknownHandler (message: Message, clientData: IRCClient) = ()
    /// RPL_LUSERCHANNELS handler
    let rplLUserChannelsHandler (message: Message, clientData: IRCClient) = ()
    /// RPL_LUSERME handler
    let rplLUserMeHandler (message: Message, clientData: IRCClient) = ()

    /// RPL_ISUPPORT handler
    let rplISupportHandler (message: Message, clientData: IRCClient) =
        let paramsLen = message.Params.Value.Value.Length
        let wantedParams = message.Params.Value.Value.[1..paramsLen-2]

        let features =
            [| 
                for param in wantedParams ->
                    let paramSplit = param.Value.Split('=')
                    match paramSplit.Length with
                    | 1 -> (paramSplit.[0], "")
                    | _ -> (paramSplit.[0], paramSplit.[1])
            |]
        
        serverFeaturesHandler (features, clientData)
        clientData.ServerFeatures <- clientData.ServerFeatures |> Map.toArray |> Array.append features |> Array.distinct |> Map.ofArray

//#region RPL_LOCALUSERS/RPL_GLOBALUSERS handlers
    let currentUsersRegex = @"[uU]sers.+?(\d)"
    let maxUsersRegex = @"[mM]ax.+?(\d+)"

    let getCurrentAndMaxUsers param =
        let currentUsers =
            matchRegexGroup param currentUsersRegex
            |> function
            | Some r -> 
                try
                    int (r.[0].Groups.[1].Value)
                with
                | _ -> -1
            | None -> -1
            
        let maxUsers =
            matchRegexGroup param maxUsersRegex
            |> function
            | Some r -> 
                try
                    int (r.[0].Groups.[1].Value)
                with
                | _ -> -1
            | None -> -1
        
        (currentUsers, maxUsers)

    /// RPL_LOCALUSERS handler
    let rplLocalUsersHandler (message: Message, clientData: IRCClient) =
        let wantedParam = message.Params.Value.Value.[1].Value

        clientData.ServerInfo <- {clientData.ServerInfo with LocalUserInfo = getCurrentAndMaxUsers wantedParam}

    /// RPL_GLOBALUSERS handler
    let rplGlobalUsersHandler (message: Message, clientData: IRCClient) =
        let wantedParam = message.Params.Value.Value.[1].Value

        clientData.ServerInfo <- {clientData.ServerInfo with GlobalUserInfo = getCurrentAndMaxUsers wantedParam}
//#endregion RPL_LOCALUSERS/RPL_GLOBALUSERS handlers

//#region MOTD handlers
    let rplMotdStartHandler (message: Message, clientData: IRCClient) =
        ()

    let rplMotdHandler (message: Message, clientData: IRCClient) =
        let wantedParam = message.Params.Value.Value.[1].Value

        clientData.ServerMOTD <- clientData.ServerMOTD @ [wantedParam]

    let rplEndOfMotdHandler (message: Message, clientData: IRCClient) =
        ()
//#endregion MOTD handlers

//#region Channel messages
    let rplNamReplyHandler (message: Message, clientData: IRCClient) =
        let channelStatus = parseChannelStatus (message.Params.Value.Value.[1].Value)
        let channelName = message.Params.Value.Value.[2].Value
        let channelUsers = [| for cu in message.Params.Value.Value.[3..] -> cu.Value |]

        clientData.GetChannelInfo channelName
        |> function
        | Some ci ->
            {ci with UserCount = ci.UserCount + channelUsers.Length; Users = ci.Users |> Array.append channelUsers}
        | None -> 
            {Name = channelName; Status = channelStatus; Topic = ""; UserCount = channelUsers.Length; Users = channelUsers}
        |> clientData.SetChannelInfo channelName

    let rplEndOfNamesHandler (message: Message, clientData: IRCClient) =
        ()

    let rplTopicHandler (message: Message, clientData: IRCClient) =
        let channelName = message.Params.Value.Value.[1].Value
        let channelTopic = message.Params.Value.Value.[2].Value

        clientData.GetChannelInfo channelName
        |> function
        | Some ci ->
            {ci with Topic = channelTopic}
        | None ->
            {Name = ""; Status = ""; Topic = channelTopic; UserCount = 0; Users = [||]}
        |> clientData.SetChannelInfo channelName

    let rplAwayHandler (message: Message, clientData: IRCClient) =
        ()
//#endregion
