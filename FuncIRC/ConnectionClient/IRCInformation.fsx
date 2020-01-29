#load "../IRC/MessageTypes.fsx"
#load "ConnectionClient.fsx"

namespace FuncIRC

open MessageTypes
open ConnectionClient

module IRCInformation =
    type IRCUserInfo =
        {
            Source: Source
        }

    type IRCServerInfo =
        {
            Name: string
            GlobalUserCount: int
            LocalUserCount: int
        }

    type IRCChannelInfo =  
        {
            Name: string
            UserCount: int
        }