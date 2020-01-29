#load "../ConnectionClient/IRCClientData.fsx"
#load "../Utils/RegexHelpers.fsx"
#load "MessageTypes.fsx"
#load "NumericReplies.fsx"

namespace FuncIRC

open MessageTypes
open IRCClientData
open NumericReplies
open RegexHelpers
open System

#if !DEBUG
module internal MessageHandlers =
#else
module MessageHandlers =
#endif

    /// PONG message const 
    let private pongMessage = Message.NewSimpleMessage (Some (Verb "PONG")) None
    /// PING message handler
    let pongMessageHandler (message: Message, clientData: IRCClientData) =
        clientData.AddOutMessage pongMessage

    /// RPL_WELCOME handler
    let rplWelcomeHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_WELCOME: %s" message.Params.Value.Value.[1].Value

    /// RPL_YOURHOST handler
    let rplYourHostHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_YOURHOST: %s" message.Params.Value.Value.[1].Value

//#region RPL_CREATED handler
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
        printfn "RPL_MYINFO: "

    /// RPL_LUSERCLIENT handler
    let rplLUserClientHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_LUSERCLIENT: %A" [| for p in message.Params.Value.Value -> p.Value |]

    /// RPL_LUSERUNKNOWN handler
    let rplLUserUnknownHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_LUSERUNKNOWN: %A" [| for p in message.Params.Value.Value -> p.Value |]

    /// RPL_LUSERCHANNELS handler
    let rplLUserChannelsHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_LUSERCHANNELS: %A" [| for p in message.Params.Value.Value -> p.Value |]

    /// RPL_LUSERME handler
    let rplLUserMeHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_LUSERME: %A" [| for p in message.Params.Value.Value -> p.Value |]

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
    let currentUsersRegex = @"users.+?(\d)"
    let maxUsersRegex = @"[mM]ax.+?(\d+)"

    /// RPL_LOCALUSERS handler
    let rplLocalUsersHandler (message: Message, clientData: IRCClientData) =
        let wantedParam = message.Params.Value.Value.[1].Value
       
        let currentLocalUsers =
            matchRegexGroup wantedParam currentUsersRegex
            |> function
            | Some r -> int (r.[0].Groups.[1].Value)
            | None -> -1
            
        let maxLocalUsers =
            matchRegexGroup wantedParam maxUsersRegex
            |> function
            | Some r -> int (r.[0].Groups.[1].Value)
            | None -> -1

        clientData.ServerInfo <- {clientData.ServerInfo with LocalUserInfo = (currentLocalUsers, maxLocalUsers)}

    /// RPL_GLOBALUSERS handler
    let rplGlobalUsersHandler (message: Message, clientData: IRCClientData) =
        let wantedParam = message.Params.Value.Value.[1].Value
       
        let currentGlobalUsers =
            matchRegexGroup wantedParam currentUsersRegex
            |> function
            | Some r -> int (r.[0].Groups.[1].Value)
            | None -> -1
            
        let maxGlobalUsers =
            matchRegexGroup wantedParam maxUsersRegex
            |> function
            | Some r -> int (r.[0].Groups.[1].Value)
            | None -> -1

        clientData.ServerInfo <- {clientData.ServerInfo with GlobalUserInfo = (currentGlobalUsers, maxGlobalUsers)}
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