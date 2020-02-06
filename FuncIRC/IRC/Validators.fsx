#load "../ConnectionClient/IRCClientData.fsx"
#load "../Utils/RegexHelpers.fsx"
#load "../Utils/StringHelpers.fsx"
#load "MessageTypes.fsx"
#load "IrcUtils.fsx"

namespace FuncIRC

module Validators =
    open System
    open RegexHelpers
    open StringHelpers
    open IRCClientData
    open MessageTypes
    open IrcUtils

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

    let private invalidTagKeyCharacters = [| '/'; '-'; '.' |]

    /// Validates a key of a tag
    let validateTagKey (clientData: IRCClientData) (key: string) =
        match key with
        | "" -> false
        | _ when key.Length > clientData.GetServerInfo.MaxKeyLength -> false
        | _ when not (stringIsOnlyAlphaNumericExcept key invalidTagKeyCharacters) -> false
        | _ -> true

    let private invalidTagValueCharacters = [| crCharacter; lfCharacter; ';'; ' '; nulCharacter; bellCharacter; spaceCharacter |]

    /// Validates the value of a tag
    let validateTagValue (clientData: IRCClientData) (value: string) =
        match value with
        | _ when value.Length > clientData.GetServerInfo.MaxKeyLength -> false
        | _ when not (stringDoesNotContain value invalidTagValueCharacters) -> false
        | _ -> true

    /// Validate length of message string
    let validateMessageString (clientData: IRCClientData) (message: string) =
        match message with
        | _ when message.Length > clientData.GetServerInfo.LineLength -> false
        | _ -> true

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

    let validateChannelsString (clientData: IRCClientData) (channelsString: string) =
        ()