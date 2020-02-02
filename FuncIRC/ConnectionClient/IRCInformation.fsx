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
            Name: string;
            Created: DateTime;
            GlobalUserInfo: int * int;
            LocalUserInfo: int * int;
        }

    type IRCChannelInfo =
        {
            Name: string
            Status: string
            Topic: string
            UserCount: int
            Users: string array
        }

    type IRCServerChannels =
        {
            mutable Channels: Map<string, IRCChannelInfo>
        }

    type IRCServerMOTD = 
        | MOTD of string list
        member x.Value = let (MOTD motd) = x in motd


    type IRCServerFeatures =
        | Features of Map<string, string>
        member x.Value = let (Features features) = x in features

//#region Defaults
    let default_IRCServerInfo = {Name = "DEFAULT"; Created = DateTime.MinValue; GlobalUserInfo = (-1, -1); LocalUserInfo = (-1, -1)}
//#endregion