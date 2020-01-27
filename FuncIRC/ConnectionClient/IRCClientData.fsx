#load "ConnectionClient.fsx"
#load "MessageSubscription.fsx"
#load "MessageQueue.fsx"

namespace FuncIRC

open System.Threading
open ConnectionClient
open MessageSubscription
open MessageQueue

module IRCClientData =
    type IRCClientData = 
        {
            TokenSource: CancellationTokenSource
            SubscriptionQueue: MessageSubscriptionQueue
            OutQueue: MessageQueue
            InQueue: MessageQueue
        }