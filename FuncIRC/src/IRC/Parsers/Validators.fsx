#load "../../Client/IRCClientData.fsx"
#load "../../Utils/RegexHelpers.fsx"
#load "../../Utils/StringHelpers.fsx"
#load "../../Utils/IrcUtils.fsx"
#load "../Types/MessageTypes.fsx"

namespace FuncIRC

#if !DEBUG
module internal Validators =
#else
module Validators =
#endif
    open System
    open RegexHelpers
    open StringHelpers
    open IRCClientData
    open MessageTypes
    open IrcUtils

//#region Source related
    /// Validates the hostname part of a Source
    /// <returns> true if it was valid </returns>
    let validateHostname (clientData: IRCClientData) (hostname: string) =
        match hostname with
        | "" -> false
        | _ when hostname.IndexOf('.') <> -1 -> 
            match () with
            | _ when hostname.Length > clientData.ServerInfo.MaxHostLength -> false
            | _ when Char.IsLetterOrDigit (hostname.[0]) -> true
            | _ -> false
        | _ -> false

    /// Validates the nick string
    /// <returns> true if it was valid </returns>
    let validateNick (clientData: IRCClientData) (nick: string) =
        if      nick = "" then false
        else if nick.IndexOf (' ') <> -1 then false
        else if nick.Length > clientData.GetServerInfo.MaxNickLength then false
        else true

    /// Validates the user string
    /// <returns> true if it was valid </returns>
    let validateUser (clientData: IRCClientData) (user: string) =
        if      user = "" then false
        else if user.IndexOf (' ') <> -1 then false
        else if user.Length > clientData.GetServerInfo.MaxUserLength then false
        else true

    /// Validates a Source
    /// <returns> true if it was valid </returns>
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
    /// <returns> true if it was valid </returns>
    let validateTagKey (clientData: IRCClientData) (key: string) =
        match key with
        | "" -> false
        | _ when key.Length > clientData.GetServerInfo.MaxKeyLength -> false
        | _ when not (stringIsOnlyAlphaNumericExcept key invalidTagKeyCharacters) -> false
        | _ -> true

    let private invalidTagValueCharacters = [| crCharacter; lfCharacter; ';'; ' '; nulCharacter; bellCharacter; spaceCharacter |]

    /// Validates the value of a tag
    /// <returns> true if it was valid </returns>
    let validateTagValue (clientData: IRCClientData) (value: string) =
        match value with
        | _ when value.Length > clientData.GetServerInfo.MaxKeyLength -> false
        | _ when not (stringDoesNotContain value invalidTagValueCharacters) -> false
        | _ -> true

    /// Validate length of message string
    /// <returns> true if it was valid </returns>
    let validateMessageString (clientData: IRCClientData) (message: string) =
        match message with
        | _ when message.Length > clientData.GetServerInfo.LineLength -> false
        | _ -> true

    /// Validates the topic string
    /// <returns> true if it was valid </returns>
    let validateTopic (clientData: IRCClientData) (topic: string) =
        if      topic = "" then false
        else if topic.Length > clientData.GetServerInfo.MaxTopicLength then false
        else true

    /// Validates the channel string
    /// <returns> true if it was valid </returns>
    let validateChannel (clientData: IRCClientData) (channel: string) =
        if      channel = "" then false
        else if channel.Length > clientData.GetServerInfo.MaxChannelLength then false
        else

        let channelPrefix = channel.[0]

        if Map.containsKey channelPrefix clientData.GetServerInfo.ChannelPrefixes |> not then false
        else true

    /// Validates a list of channels in a comma separated string
    /// <returns> true if it was valid </returns>
    let validateChannelsString (clientData: IRCClientData) (channelsString: string) =
        let channelsSplit = channelsString.Split(',')

        match channelsSplit.Length with
        | length when length > clientData.GetServerInfo.MaxTargets -> false
        | 1 -> validateChannel clientData channelsString
        | _ -> 
            channelsSplit
            |> Array.forall
                ( fun ch -> validateChannel clientData ch )

    /// Validates a string of comma separated nicks
    /// <returns> true if it was valid </returns>
    let validateNicksString (clientData: IRCClientData) (nicksString: string) =
        let nicksSplit = nicksString.Split(',')

        match nicksSplit.Length with
        | length when length > clientData.GetServerInfo.MaxTargets -> false
        | 1 -> validateNick clientData nicksString
        | _ -> 
            nicksSplit
            |> Array.forall
                ( fun nick -> validateNick clientData nick )
        