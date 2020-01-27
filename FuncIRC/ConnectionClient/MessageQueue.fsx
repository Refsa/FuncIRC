#load "../IRC/MessageTypes.fsx"

namespace FuncIRC

open MessageTypes

module MessageQueue =

    [<Sealed>]
    type MessageQueue() =
        let mutable messages: Message list = []

        member this.Count = messages.Length

        member this.AddMessage (message: Message) =
            messages <- message :: messages

        member this.AddMessages (messageList: Message list) =
            messages <- messages @ messageList

        member this.PopMessage: Message =
            let first = messages.Head
            messages <- messages.Tail
            first
        
        member this.PopMessages: Message list = 
            let storedMessages = messages
            messages <- []
            storedMessages