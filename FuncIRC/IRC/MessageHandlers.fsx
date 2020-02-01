#load "../ConnectionClient/IRCClientData.fsx"
#load "../Utils/RegexHelpers.fsx"
#load "MessageTypes.fsx"
#load "MessageParserInternals.fsx"
#load "NumericReplies.fsx"

namespace FuncIRC

open MessageTypes
open IRCClientData
open IRCInformation
open NumericReplies
open MessageParserInternals
open RegexHelpers
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
    let pongMessageHandler (message: Message, clientData: IRCClientData) =
        clientData.AddOutMessage pongMessage
//#endregion PING message handler

    /// RPL_WELCOME handler
    let rplWelcomeHandler (message: Message, clientData: IRCClientData) =
        ()
        //printfn "RPL_WELCOME: %s" message.Params.Value.Value.[1].Value

    /// RPL_YOURHOST handler
    let rplYourHostHandler (message: Message, clientData: IRCClientData) =
        ()
        //printfn "RPL_YOURHOST: %s" message.Params.Value.Value.[1].Value

//#region RPL_CREATED handler
    /// Regex to capture DateTimes in the format of: 23:25:21 Jan 24 2020
    /// TODO: Add testing of different DateTime formats
    let dateTimeRegex = @"(\d{2}:\d{2}:\d{2}.+)"
    /// RPL_CREATED handler
    let rplCreatedHandler (message: Message, clientData: IRCClientData) =
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
    let rplMyInfoHandler (message: Message, clientData: IRCClientData) =
        ()
        //printfn "RPL_MYINFO: "

    /// RPL_LUSERCLIENT handler
    let rplLUserClientHandler (message: Message, clientData: IRCClientData) =
        ()
        //printfn "RPL_LUSERCLIENT: %A" [| for p in message.Params.Value.Value -> p.Value |]

    /// RPL_LUSERUNKNOWN handler
    let rplLUserUnknownHandler (message: Message, clientData: IRCClientData) =
        ()
        //printfn "RPL_LUSERUNKNOWN: %A" [| for p in message.Params.Value.Value -> p.Value |]

    /// RPL_LUSERCHANNELS handler
    let rplLUserChannelsHandler (message: Message, clientData: IRCClientData) =
        ()
        //printfn "RPL_LUSERCHANNELS: %A" [| for p in message.Params.Value.Value -> p.Value |]

    /// RPL_LUSERME handler
    let rplLUserMeHandler (message: Message, clientData: IRCClientData) =
        ()
        //printfn "RPL_LUSERME: %A" [| for p in message.Params.Value.Value -> p.Value |]

    /// RPL_ISUPPORT handler
    let rplISupportHandler (message: Message, clientData: IRCClientData) =
        let paramsLen = message.Params.Value.Value.Length
        let wantedParams = message.Params.Value.Value.[1..paramsLen-2]

        let features =
            [| 
                for param in wantedParams ->
                    let paramSplit = param.Value.Split('=')
                    (paramSplit.[0], paramSplit.[1]) 
            |]
        
        clientData.ServerFeatures <- clientData.ServerFeatures |> Array.append features

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
    let rplLocalUsersHandler (message: Message, clientData: IRCClientData) =
        let wantedParam = message.Params.Value.Value.[1].Value

        clientData.ServerInfo <- {clientData.ServerInfo with LocalUserInfo = getCurrentAndMaxUsers wantedParam}

    /// RPL_GLOBALUSERS handler
    let rplGlobalUsersHandler (message: Message, clientData: IRCClientData) =
        let wantedParam = message.Params.Value.Value.[1].Value

        clientData.ServerInfo <- {clientData.ServerInfo with GlobalUserInfo = getCurrentAndMaxUsers wantedParam}
//#endregion RPL_LOCALUSERS/RPL_GLOBALUSERS handlers

//#region MOTD handlers
    let rplMotdStartHandler (message: Message, clientData: IRCClientData) =
        ()

    let rplMotdHandler (message: Message, clientData: IRCClientData) =
        let wantedParam = message.Params.Value.Value.[1].Value

        clientData.ServerMOTD <- clientData.ServerMOTD @ [wantedParam]

    let rplEndOfMotdHandler (message: Message, clientData: IRCClientData) =
        ()
//#endregion MOTD handlers

//#region Channel messages
    let rplNamReplyHandler (message: Message, clientData: IRCClientData) =
        let channelStatus = parseChannelStatus (message.Params.Value.Value.[1].Value)
        let channelName = message.Params.Value.Value.[2].Value
        let channelUsers = [| for cu in message.Params.Value.Value.[3..] -> cu.Value |]

        clientData.GetChannelInfo channelName
        |> function
        | Some ci ->
            {ci with UserCount = ci.UserCount + channelUsers.Length; Users = ci.Users |> Array.append channelUsers}
        | None -> 
            {Name = channelName; Status = channelStatus; UserCount = channelUsers.Length; Users = channelUsers}
        |> clientData.SetChannelInfo channelName

    let rplEndOfNamesHandler (message: Message, clientData: IRCClientData) =
        ()

//#endregion

//#region Error numerics
    // General error message
    let errNeedMoreParamsHandler (message: Message, clientData: IRCClientData) =
        let errorResponse = message.Params.Value.Value.[1].Value + ": Did not have enough parameters"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to USER and PASS verb
    let errAlreadyRegisteredHandler (message: Message, clientData: IRCClientData) =
        let errorResponse = "Already Registered: You have already registered with the server, cant change details at this time"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to NICK verb
    let errNoNicknameGivenHandler (message: Message, clientData: IRCClientData) =
        let errorResponse = "No Nickname was supplied to the NICK command"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to NICK verb
    let errErroneusNicknameHandler (message: Message, clientData: IRCClientData) =
        let errorResponse = "Nickname [" + message.Params.Value.Value.[1].Value + "] was not accepted by server: Erroneus Nickname"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to NICK verb
    let errNicknameInUseHandler (message: Message, clientData: IRCClientData) =
        let errorResponse = "Nickname [" + message.Params.Value.Value.[1].Value + "] is already in use on server"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to NICK verb
    let errNickCollisionHandler (message: Message, clientData: IRCClientData) =
        let errorResponse = "Nickname [" + message.Params.Value.Value.[1].Value + "] threw a nick collision response from server"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errNoSuchChannelHandler (message: Message, clientData: IRCClientData) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] does not exist"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errTooManyChannelsHandler (message: Message, clientData: IRCClientData) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] You have joined too many channels"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errBadChannelKeyHandler  (message: Message, clientData: IRCClientData) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] cannot join channel, bad channel key"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errBannedFromChanHandler (message: Message, clientData: IRCClientData) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] you are banned from this channel"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errChannelIsFullHandler (message: Message, clientData: IRCClientData) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] is full, you cannot join it"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errInviteOnlyChanHandler (message: Message, clientData: IRCClientData) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] is invite only"
        clientData.ErrorNumericReceivedTrigger (errorResponse)
//#endregion Error numerics