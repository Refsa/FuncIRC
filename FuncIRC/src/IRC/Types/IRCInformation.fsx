#load "MessageTypes.fsx"

namespace FuncIRC

open MessageTypes
open System

module IRCInformation =
    /// <summary>
    /// Casemapping enum
    /// </summary>
    type Casemapping =
        | ASCII
        | RFC1459
        | RFC1459Strict
        | RFC7613
        | Unknown

    /// <summary>
    /// Stores the relevant information about a user
    /// </summary>
    type IRCUserInfo =
        {
            Source: Source
        }

    /// <summary>
    /// Record to store the symbols related to channel modes of a server
    /// </summary>
    type IRCChannelModes =
        {
            TypeA: string
            TypeB: string
            TypeC: string
            TypeD: string
        }

    /// <summary>
    /// Record to store the symbols related to user modes of a server
    /// </summary>
    type IRCUserModes =
        {
            Founder: string
            Protected: string
            Operator: string
            Halfop: string
            Voice: string
        }

    /// <summary>
    /// TODO: Refactor and split up into more readable pieces
    /// General information for a network/server
    /// </summary>
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

    /// <summary>
    /// General information about a channel
    /// </summary>
    type IRCChannelInfo =
        {
            Name: string
            Status: string
            Topic: string
            UserCount: int
            Users: string array
        }

    /// <summary>
    /// Container for a Map of channel names to IRCChannelInfo records
    /// </summary>
    type IRCServerChannels =
        {
            mutable Channels: Map<string, IRCChannelInfo>
        }

    /// <summary>
    /// Container of the MOTD for a server
    /// </summary>
    type IRCServerMOTD = 
        | MOTD of string list
        member x.Value = let (MOTD motd) = x in motd

    /// <summary>
    /// Container of the raw server feature data reported by RPL_ISUPPORT
    /// </summary>
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