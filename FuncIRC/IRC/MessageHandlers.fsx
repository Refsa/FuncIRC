#load "../ConnectionClient/IRCClientData.fsx"
#load "../Utils/RegexHelpers.fsx"
#load "MessageTypes.fsx"
#load "NumericReplies.fsx"

namespace FuncIRC

open MessageTypes
open IRCClientData
open NumericReplies
open RegexHelpers

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

    /// RPL_CREATED handler
    let rplCreatedHandler (message: Message, clientData: IRCClientData) =
        printfn "RPL_CREATED: %s" message.Params.Value.Value.[1].Value

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

//#region MOTD handlers
    let rplMotdStartHandler (message: Message, clientData: IRCClientData) =
        ()

    let rplMotdHandler (message: Message, clientData: IRCClientData) =
        ()

    let rplEndOfMotdHandler (message: Message, clientData: IRCClientData) =
        ()
//#endregion MOTD handlers