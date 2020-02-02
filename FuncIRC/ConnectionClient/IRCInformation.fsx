#load "../IRC/MessageTypes.fsx"
#load "ConnectionClient.fsx"

namespace FuncIRC

open MessageTypes
open ConnectionClient
open System

module IRCInformation =
    type Casemapping =
        | ASCII
        | RFC1459
        | RFC1459Strict
        | RFC7613
        | Unknown

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

            Casemapping: Casemapping
            LineLength: int

            ChannelPrefixes: char array

            MaxChannelLength: int
            MaxTargets: int
            MaxAwayLength: int
            MaxKickLength: int
            MaxTopicLength: int
            MaxUserLength: int
            MaxNickLength: int
            MaxModes: int
            MaxKeyLength: int
            MaxHostLength: int
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
    let default_IRCServerInfo = 
        {
            Name = "DEFAULT"; 
            Created = DateTime.MinValue; 
            GlobalUserInfo = (-1, -1); 
            LocalUserInfo = (-1, -1); 
            Casemapping = Casemapping.Unknown;
            LineLength = 512;
            ChannelPrefixes = Array.empty;
            MaxChannelLength = 32;
        }
//#endregion