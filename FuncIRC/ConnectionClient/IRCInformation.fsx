#load "../IRC/MessageTypes.fsx"
#load "ConnectionClient.fsx"

namespace FuncIRC

open MessageTypes
open ConnectionClient
open System

module IRCInformation =
    type IRCUserInfo =
        {
            Source: Source
        }

    type IRCServerInfo =
        {
            Name: string
            Created: DateTime
            GlobalUserCount: int
            LocalUserCount: int
        }

    type IRCChannelInfo =  
        {
            Name: string
            UserCount: int
        }