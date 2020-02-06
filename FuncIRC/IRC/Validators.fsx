#load "../ConnectionClient/IRCClientData.fsx"
#load "../Utils/RegexHelpers.fsx"
#load "../Utils/StringHelpers.fsx"
#load "MessageTypes.fsx"

namespace FuncIRC

module Validators =
    open System
    open RegexHelpers
    open StringHelpers
    open IRCClientData
    open MessageTypes


//#region Source related
    /// Validates the hostname part of a Source
    let validateHostname (clientData: IRCClientData) (hostname: string) =
        match hostname with
        | "" -> false
        | _ when hostname.IndexOf('.') <> -1 -> 
            match () with
            | _ when hostname.Length > clientData.ServerInfo.MaxHostLength -> false
            | _ when Char.IsLetterOrDigit (hostname.[0]) -> true
            | _ -> false
        |_ -> false

    /// Validates the nick string
    let validateNick (clientData: IRCClientData) (nick: string) =
        if      nick = "" then false
        else if nick.IndexOf (' ') <> -1 then false
        else if nick.Length > clientData.GetServerInfo.MaxNickLength then false
        else true

    /// Validates the user string
    let validateUser (clientData: IRCClientData) (user: string) =
        if      user = "" then false
        else if user.IndexOf (' ') <> -1 then false
        else if user.Length > clientData.GetServerInfo.MaxUserLength then false
        else true

    /// Validates a Source
    let validateSource (clientData: IRCClientData) (source: Source) =
        let hostname = stringFromStringOption source.Host
        let nick = stringFromStringOption source.Nick
        let user = stringFromStringOption source.User

        validateHostname clientData hostname &&
        validateNick clientData nick &&
        validateUser clientData user 

//#endregion Source related

    /// Validates the topic string
    let validateTopic (clientData: IRCClientData) (topic: string) =
        if      topic = "" then false
        else if topic.Length > clientData.GetServerInfo.MaxTopicLength then false
        else true

    /// Validates the channel string
    let validateChannel (clientData: IRCClientData) (channel: string) =
        if      channel = "" then false
        else if channel.Length > clientData.GetServerInfo.MaxChannelLength then false
        else

        let channelPrefix = channel.[0]

        if Map.containsKey channelPrefix clientData.GetServerInfo.ChannelPrefixes |> not then false
        else true