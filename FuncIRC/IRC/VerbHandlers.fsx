#load "MessageTypes.fsx"
#load "NumericReplies.fsx"
#load "../ConnectionClient/ConnectionClient.fsx"

namespace FuncIRC

module VerbHandlers =
    open MessageTypes
    open NumericReplies
    open ConnectionClient

    let noCallback() = ""

    let verbPingHandler(): string =
        "PONG"

    /// Binds NumericReplies to delegate handlers
    /// Handlers return the response verb and parameters
    let verbHandlers =
        [
            NumericReplies.PING, verbPingHandler
        ] |> Map.ofList

    let (|IsNumeric|IsVerbName|) (verb: Verb) =
        let numeric = verbToInt verb

        match numeric with
        | Some numeric -> IsNumeric numeric
        | None -> IsVerbName verb.Value

    let (|IsPing|IsNotice|IsPrivMsg|UnknownVerbName|) (verb: string) =
        match verb with
        | "PING" -> IsPing verbHandlers.[NumericReplies.PING]
        | "NOTICE" -> IsNotice noCallback
        | "PRIVMSG" -> IsPrivMsg noCallback
        | _ -> UnknownVerbName noCallback