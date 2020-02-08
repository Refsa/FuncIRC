#load "../IRC/MessageTypes.fsx"

namespace FuncIRC

open MessageTypes
open System

module IRCInformation =
    /// Casemapping enum
    type Casemapping =
        | ASCII
        | RFC1459
        | RFC1459Strict
        | RFC7613
        | Unknown

    /// Stores the relevant information about a user
    type IRCUserInfo =
        {
            Source: Source
        }

    /// Record to store the symbols related to channel modes of a server
    type IRCChannelModes =
        {
            TypeA: string
            TypeB: string
            TypeC: string
            TypeD: string
        }

    /// Record to store the symbols related to user modes of a server
    type IRCUserModes =
        {
            Founder: string
            Protected: string
            Operator: string
            Halfop: string
            Voice: string
        }

    /// TODO: Refactor and split up into more readable pieces
    /// General information for a network/server
    type IRCServerInfo =
        {
            Name: string;
            Created: DateTime;
            GlobalUserInfo: int * int;
            LocalUserInfo: int * int;

            Casemapping: Casemapping
            LineLength: int

            /// Channel prefix as key and limit as value
            ChannelPrefixes: Map<char, int>
            ChannelModes: IRCChannelModes

            UserModes: IRCUserModes
            StatusMessageModes: char array

            Safelist: bool
            SearchExtensions: char array
            
            MaxTypeAModes: Map<char, int>
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

    /// General information about a channel
    type IRCChannelInfo =
        {
            Name: string
            Status: string
            Topic: string
            UserCount: int
            Users: string array
        }

    /// Container for a Map of channel names to IRCChannelInfo records
    type IRCServerChannels =
        {
            mutable Channels: Map<string, IRCChannelInfo>
        }

    /// Container of the MOTD for a server
    type IRCServerMOTD = 
        | MOTD of string list
        member x.Value = let (MOTD motd) = x in motd

    /// Container of the raw server feature data reported by RPL_ISUPPORT
    type IRCServerFeatures =
        | Features of Map<string, string>
        member x.Value = let (Features features) = x in features

//#region Defaults
    let default_IRCChannelModes =
        {
            TypeA = "A";
            TypeB = "B";
            TypeC = "C";
            TypeD = "D";
        }
    let default_IRCUserModes =
        {
            Founder = "";
            Protected = "";
            Operator = "";
            Halfop = "";
            Voice = "";
        }

    let default_IRCServerInfo = 
        {
            Name = "DEFAULT"; 
            Created = DateTime.MinValue; 
            GlobalUserInfo = (-1, -1); 
            LocalUserInfo = (-1, -1); 
            Casemapping = Casemapping.Unknown;
            LineLength = 512;
            
            ChannelPrefixes = Map.empty;
            ChannelModes = default_IRCChannelModes

            UserModes = default_IRCUserModes
            StatusMessageModes = Array.empty

            Safelist = false
            SearchExtensions = Array.empty

            MaxTypeAModes = Map.empty
            MaxChannelLength = 32;
            MaxTargets = 20;
            MaxAwayLength = 200;
            MaxKickLength = 255;
            MaxTopicLength = 307;
            MaxUserLength = 10;
            MaxNickLength = 30;
            MaxModes = 20;
            MaxKeyLength = 32;
            MaxHostLength = 64;
        }
//#endregion