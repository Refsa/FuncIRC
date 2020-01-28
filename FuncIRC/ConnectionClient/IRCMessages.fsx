#load "IRCClient.fsx"
#load "IRCClientData.fsx"
#load "MessageSubscription.fsx"
#load "MessageQueue.fsx"
#load "../IRC/MessageTypes.fsx"
#load "../IRC/NumericReplies.fsx"
#load "../IRC/MessageHandlers.fsx"

namespace FuncIRC

open IRCClient
open IRCClientData
open MessageTypes
open MessageSubscription
open MessageHandlers
open MessageQueue
open NumericReplies

module IRCMessages =
    /// Exception thrown when parameters to a registration message was missing
    exception RegistrationContentException

    /// Creates a registration message and sends it to the outbound message queue
    /// Subscribes to incoming VERBs related to the registration message
    let sendRegistrationMessage (clientData: IRCClientData) (nick: string, user: string, realName: string, pass: string) =
        let messages = 
            match () with
            | _ when pass <> "" && nick <> "" && user <> "" -> 
                [
                    { Tags = None; Source = None; Verb = Some (Verb "CAP"); Params = Some (toParameters [|"LS"; "302"|]) }
                    { Tags = None; Source = None; Verb = Some (Verb "PASS"); Params = Some (toParameters [|pass|]) }
                    { Tags = None; Source = None; Verb = Some (Verb "NICK"); Params = Some (toParameters [|nick|]) }
                    { Tags = None; Source = None; Verb = Some (Verb "USER"); Params = Some (toParameters [|user; "0"; "*"; realName|]) }
                ]
            | _ when nick <> "" && user <> "" -> 
                [
                    { Tags = None; Source = None; Verb = Some (Verb "CAP"); Params = Some (toParameters [|"LS"; "302"|]) }
                    { Tags = None; Source = None; Verb = Some (Verb "NICK"); Params = Some (toParameters [|nick|]) }
                    { Tags = None; Source = None; Verb = Some (Verb "USER"); Params = Some (toParameters [|user; "0"; "*"; realName|]) }
                ]
            | _ -> raise RegistrationContentException

        clientData.AddOutMessages messages

        clientData.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_WELCOME.Value)) rplWelcomeHandler)
        clientData.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_YOURHOST.Value)) rplYourHostHandler)
        clientData.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_CREATED.Value)) rplCreatedHandler)
        clientData.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_MYINFO.Value)) rplMyInfoHandler)

        // ISUPPORT HERE

        clientData.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_LUSERCLIENT.Value)) rplLUserClientHandler)
        clientData.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_LUSERUNKNOWN.Value)) rplLUserUnknownHandler)
        clientData.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_LUSERCHANNELS.Value)) rplLUserChannelsHandler)
        clientData.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_LUSERME.Value)) rplLUserMeHandler)
        clientData.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_LOCALUSERS.Value)) rplLocalUsersHandler)
        clientData.AddSubscription (MessageSubscription.NewSingle (Verb (NumericsReplies.RPL_GLOBALUSERS.Value)) rplGlobalUsersHandler)


    /// Creates a QUIT messages and adds it to the outbound message queue
    let sendQuitMessage (clientData: IRCClientData) (message: string) =
        { Tags = None; Source = None; Verb = Some (Verb "QUIT"); Params = Some (toParameters [|message|]) }
        |> clientData.AddOutMessage