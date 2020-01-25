#load "MessageTypes.fsx"
#load "NumericReplies.fsx"
#load "../ConnectionClient/ConnectionClient.fsx"

namespace FuncIRC

module VerbHandlers =
    open MessageTypes
    open NumericReplies
    open ConnectionClient


    type VerbHandlerType =
        | NotImplemented
        | Response
        | Callback

    type VerbHandler =
        {
            Type: VerbHandlerType
            Content: string
        }

    let noCallback = {Type = VerbHandlerType.NotImplemented; Content = ""}
    let noCallbackHandler() = noCallback

    let verbPingHandler(): VerbHandler =
        {Type = VerbHandlerType.Response; Content = "PONG"}

    let privMsgHandler(): VerbHandler =
        {Content = "PRIVMSG"; Type = VerbHandlerType.Callback}

    let noticeHandler(): VerbHandler =
        {Content = "NOTICE"; Type = VerbHandlerType.Callback}

    let rplWelcomeHandler(): VerbHandler =
        {Content = "RPL_WELCOME"; Type = VerbHandlerType.Callback}

    let rplYourHostHandler(): VerbHandler =
        {noCallback with Content = "RPL_YOURHOST"}

    let rplCreatedHandler(): VerbHandler =
        {noCallback with Content = "RPL_CREATED"}

    let rplMyInfoHandler(): VerbHandler =
        {noCallback with Content = "RPL_MYINFO"}

    let rplIsupportHandler(): VerbHandler =
        {noCallback with Content = "RPL_ISUPPORT"}

    let rpllUserClientHandler(): VerbHandler =
        {noCallback with Content = "RPL_LUSERCLIENT"}

    let rplLUserUnknownHandler(): VerbHandler =
        {noCallback with Content = "RPL_LUSERUNKNOWN"}

    let rplLUserChannelsHandler(): VerbHandler =
        {noCallback with Content = "RPL_LUSERCHANNELS"}

    let rplLUserMeHandler(): VerbHandler =
        {noCallback with Content = "RPL_LUSERME"}

    let rplLocalUsersHandler(): VerbHandler =
        {noCallback with Content = "RPL_LOCALUSERS"}

    let rplGlobalUsersHandler(): VerbHandler =
        {noCallback with Content = "RPL_GLOBALUSERS"}

    let rplMotdStartHandler(): VerbHandler =
        {noCallback with Content = "RPL_MOTDSTART"}

    let rplMotdHandler(): VerbHandler =
        {noCallback with Content = "RPL_MOTD"}

    let rplEndOfMotdHandler(): VerbHandler =
        {noCallback with Content = "RPL_ENDOFMOTD"}

    /// Binds NumericReplies to delegate handlers
    /// Handlers return the response verb and parameters
    let verbHandlers =
        [
            NumericReplies.PING, verbPingHandler;
            NumericReplies.RPL_WELCOME, rplWelcomeHandler;
            NumericReplies.RPL_YOURHOST, rplYourHostHandler;
            NumericReplies.RPL_CREATED, rplCreatedHandler;
            NumericReplies.RPL_MYINFO, rplMyInfoHandler;
            NumericReplies.RPL_ISUPPORT, rplIsupportHandler;
            NumericReplies.RPL_LUSERCLIENT, rpllUserClientHandler;
            NumericReplies.RPL_LUSERUNKNOWN, rplLUserUnknownHandler;
            NumericReplies.RPL_LUSERCHANNELS, rplLUserChannelsHandler;
            NumericReplies.RPL_LUSERME, rplLUserMeHandler;
            NumericReplies.RPL_LOCALUSERS, rplLocalUsersHandler;
            NumericReplies.RPL_GLOBALUSERS, rplGlobalUsersHandler;
            NumericReplies.RPL_MOTDSTART, rplMotdStartHandler;
            NumericReplies.RPL_MOTD, rplMotdHandler;
            NumericReplies.RPL_ENDOFMOTD, rplEndOfMotdHandler;
        ] |> Map.ofList

    let (|IsNumeric|IsVerbName|) (verb: Verb) =
        let numeric = verbToInt verb

        match numeric with
        | Some numeric -> IsNumeric numeric
        | None -> IsVerbName verb.Value

    /// TODO: Refactor this as there is a limit to 7 patterns on Active Patterns
    let (|IsPing|IsNotice|IsPrivMsg|IsError|IsJoin|UnknownVerbName|) (verb: string) =
        match verb with
        | "PING"    -> IsPing verbHandlers.[NumericReplies.PING]
        | "NOTICE"  -> IsNotice noticeHandler
        | "PRIVMSG" -> IsPrivMsg privMsgHandler
        | "ERROR"   -> IsError noCallbackHandler
        | "JOIN"    -> IsJoin noCallbackHandler
        | _         -> UnknownVerbName noCallbackHandler