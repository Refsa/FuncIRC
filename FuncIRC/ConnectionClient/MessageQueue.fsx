#load "../IRC/MessageTypes.fsx"

namespace FuncIRC

open MessageTypes

module MessageQueue =

    // TODO: Refactor into a generic queue type
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

    /// Checks how many items in queue, returns nothing when empty and all the content if there is one or more
    let (|EmptyQueue|SingleItemInQueue|MultipleItemsInQueue|) (queue: MessageQueue) =
        match queue.Count with
        | 0 -> EmptyQueue
        | 1 -> SingleItemInQueue queue.PopMessage
        | _ -> MultipleItemsInQueue queue.PopMessages