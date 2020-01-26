#load "../IRC/MessageTypes.fsx"
#load "ConnectionClient.fsx"

namespace FuncIRC

open MessageTypes
open ConnectionClient

module IRCInformation =
    type IRCClientInformation =
        {
            Nick: string
            User: string
            Host: string
        }