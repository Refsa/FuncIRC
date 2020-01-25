#load "MessageTypes.fsx"
#load "NumericReplies.fsx"

namespace FuncIRC

module VerbHandlers =
    open MessageTypes
    open NumericReplies

    let verbPingHandler(): string =
        "PONG"

    /// Binds NumericReplies to delegate handlers
    /// Handlers return the response verb and parameters
    let verbHandlers =
        [
            NumericReplies.PING, verbPingHandler
        ] |> Map.ofList