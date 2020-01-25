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
        | Error

    type VerbHandler =
        {
            Type: VerbHandlerType
            Content: string
            Verb: NumericsReplies
        }

    let noCallback = {Type = VerbHandlerType.NotImplemented; Content = ""; Verb = NumericsReplies.NO_NAME}
    let noCallbackHandler(parameters: Parameters option) = noCallback

    let pingHandler(parameters: Parameters option): VerbHandler =
        {Type = VerbHandlerType.Response; Content = "PONG"; Verb = NumericsReplies.MSG_PING}

    let privMsgHandler(parameters: Parameters option): VerbHandler =
        let content =
            match parameters with
            | Some parameters -> parameters.Value |> Array.last |> fun x -> x.Value
            | None -> "PRIVMSG"

        {Content = content; Type = VerbHandlerType.Callback; Verb = NumericsReplies.MSG_PRIVMSG}

    let noticeHandler(parameters: Parameters option): VerbHandler =
        let content =
            match parameters with
            | Some parameters -> parameters.Value.[0].Value
            | _ -> "NOTICE"

        {Content = content; Type = VerbHandlerType.Callback; Verb = NumericsReplies.MSG_NOTICE}

    let rplWelcomeHandler(parameters: Parameters option): VerbHandler =
        let content = 
            match parameters with
            | Some parameters -> parameters.Value.[1].Value
            | None -> "RPL_WELCOME"
            
        {Content = content; Type = VerbHandlerType.Callback; Verb = NumericsReplies.RPL_WELCOME}

    let rplYourHostHandler(parameters: Parameters option): VerbHandler =
        let content = 
            match parameters with
            | Some parameters -> parameters.Value.[1].Value.Replace(", ", "\n")
            | None -> "RPL_YOURHOST"
        {Content = content; Type = VerbHandlerType.Callback; Verb = NumericsReplies.RPL_YOURHOST}

    let rplCreatedHandler(parameters: Parameters option): VerbHandler =
        let content = 
            match parameters with
            | Some parameters -> parameters.Value.[1].Value
            | None -> "RPL_CREATED"
        {Content = content; Type = VerbHandlerType.Callback; Verb = NumericsReplies.RPL_CREATED}

    let rplMyInfoHandler(parameters: Parameters option): VerbHandler =
        let content = 
            match parameters with
            | Some parameters -> parameters.Value.[1].Value + "-" + parameters.Value.[2].Value
            | None -> "RPL_MYINFO"
        {Content = content; Type = VerbHandlerType.Callback; Verb = NumericsReplies.RPL_MYINFO}

    let rplIsupportHandler(parameters: Parameters option): VerbHandler =
        {noCallback with Content = ""; Verb = NumericsReplies.RPL_ISUPPORT}

    let rpllUserClientHandler(parameters: Parameters option): VerbHandler =
        let content = 
            match parameters with
            | Some parameters -> parameters.Value.[1].Value
            | None -> "RPL_LUSERCLIENT"
        {Content = content; Type = VerbHandlerType.Callback; Verb = NumericsReplies.RPL_LUSERCLIENT}

    let rplLUserUnknownHandler(parameters: Parameters option): VerbHandler =
        {noCallback with Content = "RPL_LUSERUNKNOWN"}

    let rplLUserChannelsHandler(parameters: Parameters option): VerbHandler =
        {noCallback with Content = "RPL_LUSERCHANNELS"}

    let rplLUserMeHandler(parameters: Parameters option): VerbHandler =
        {noCallback with Content = "RPL_LUSERME"}

    let rplLocalUsersHandler(parameters: Parameters option): VerbHandler =
        {noCallback with Content = "RPL_LOCALUSERS"}

    let rplGlobalUsersHandler(parameters: Parameters option): VerbHandler =
        {noCallback with Content = "RPL_GLOBALUSERS"}

    let rplMotdStartHandler(parameters: Parameters option): VerbHandler =
        {noCallback with Content = "RPL_MOTDSTART"}

    let rplMotdHandler(parameters: Parameters option): VerbHandler =
        {noCallback with Content = "RPL_MOTD"}

    let rplEndOfMotdHandler(parameters: Parameters option): VerbHandler =
        {noCallback with Content = "RPL_ENDOFMOTD"}

    /// Binds NumericReplies to delegate handlers
    /// Handlers return the response verb and parameters
    let verbHandlers =
        [
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
        | "PING"    -> IsPing pingHandler
        | "NOTICE"  -> IsNotice noticeHandler
        | "PRIVMSG" -> IsPrivMsg privMsgHandler
        | "ERROR"   -> IsError noCallbackHandler
        | "JOIN"    -> IsJoin noCallbackHandler
        | _         -> UnknownVerbName noCallbackHandler