#load "../../Client/IRCClient.fsx"
#load "../Types/MessageTypes.fsx"

namespace FuncIRC

open MessageTypes
open IRCClient

#if !DEBUG
module internal ErrorNumericsHandlers =
#else
module ErrorNumericsHandlers =
#endif

//#region Error numerics
    // General error message
    let errNeedMoreParamsHandler (message: Message, clientData: IRCClient) =
        let errorResponse = message.Params.Value.Value.[1].Value + ": Did not have enough parameters"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to USER and PASS verb
    let errAlreadyRegisteredHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "Already Registered: You have already registered with the server, cant change details at this time"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to NICK verb
    let errNoNicknameGivenHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "No Nickname was supplied to the NICK command"
        clientData.SetUserInfoSelf {clientData.GetUserInfoSelf with Nick = ""}
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to NICK verb
    let errErroneusNicknameHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "Nickname [" + message.Params.Value.Value.[1].Value + "] was not accepted by server: Erroneus Nickname"
        clientData.SetUserInfoSelf {clientData.GetUserInfoSelf with Nick = ""}
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to NICK verb
    let errNicknameInUseHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "Nickname [" + message.Params.Value.Value.[1].Value + "] is already in use on server"
        clientData.SetUserInfoSelf {clientData.GetUserInfoSelf with Nick = ""}
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to NICK verb
    let errNickCollisionHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "Nickname [" + message.Params.Value.Value.[1].Value + "] threw a nick collision response from server"
        clientData.SetUserInfoSelf {clientData.GetUserInfoSelf with Nick = ""}
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errNoSuchChannelHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] does not exist"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errTooManyChannelsHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] You have joined too many channels"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errBadChannelKeyHandler  (message: Message, clientData: IRCClient) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] cannot join channel, bad channel key"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errBannedFromChanHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] you are banned from this channel"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errChannelIsFullHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] is full, you cannot join it"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to JOIN verb
    let errInviteOnlyChanHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] is invite only"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to PRIVMSG verb
    let errNoSuchNickHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] user does not exist in channel"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to PRIVMSG verb
    let errNoSuchServerHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "[" + message.Params.Value.Value.[1].Value + "] does not exist"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to PRIVMSG verb
    let errCannotSendToChanHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "Cannot send to [" + message.Params.Value.Value.[1].Value + "]"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to PRIVMSG verb
    let errTooManyTargetsHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "Too many targets for PRIVMSG"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to PRIVMSG verb
    let errNoReceipientHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "No receipient for PRIVMSG"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to PRIVMSG verb
    let errNoTextToSendHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "No text to send in PRIVMSG"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to PRIVMSG verb
    let errNoTopLevelHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "No top level for PRIVMSG"
        clientData.ErrorNumericReceivedTrigger (errorResponse)

    /// Related to PRIVMSG verb
    let errWildTopLevelHandler (message: Message, clientData: IRCClient) =
        let errorResponse = "Wild top level for PRIVMSG"
        clientData.ErrorNumericReceivedTrigger (errorResponse)
//#endregion Error numerics