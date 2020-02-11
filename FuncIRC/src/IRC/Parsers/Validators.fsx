#load "../../Client/IRCClient.fsx"
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
    open IRCClient
    open MessageTypes
    open IrcUtils

//#region Source related
    /// <summary>
    /// Validates the hostname part of a Source
    /// </summary>
    /// <returns> true if it was valid </returns>
    let validateHostname (clientData: IRCClient) (hostname: string) =
        match hostname with
        | "" -> false
        | _ when hostname.IndexOf('.') <> -1 -> 
            match () with
            | _ when hostname.Length > clientData.ServerInfo.MaxHostLength -> false
            | _ when Char.IsLetterOrDigit (hostname.[0]) -> true
            | _ -> false
        | _ -> false

    /// <summary>
    /// Validates the nick string
    /// </summary>
    /// <returns> true if it was valid </returns>
    let validateNick (clientData: IRCClient) (nick: string) =
        if      nick = "" then false
        else if nick.IndexOf (' ') <> -1 then false
        else nick.Length <= clientData.GetServerInfo.MaxNickLength

    /// <summary>
    /// Validates the user string
    /// </summary>
    /// <returns> true if it was valid </returns>
    let validateUser (clientData: IRCClient) (user: string) =
        if      user = "" then false
        else if user.IndexOf (' ') <> -1 then false
        else user.Length <= clientData.GetServerInfo.MaxUserLength

    /// <summary>
    /// Validates a Source
    /// </summary>
    /// <returns> true if it was valid </returns>
    let validateSource (clientData: IRCClient) (source: Source) =
        let hostname = stringFromStringOption source.Host
        let nick = stringFromStringOption source.Nick
        let user = stringFromStringOption source.User

        validateHostname clientData hostname &&
        validateNick clientData nick &&
        validateUser clientData user 
//#endregion Source related

    let private invalidTagKeyCharacters = [| '/'; '-'; '.' |]

    /// <summary>
    /// Validates a key of a tag
    /// </summary>
    /// <returns> true if it was valid </returns>
    let validateTagKey (clientData: IRCClient) (key: string) =
        match key with
        | "" -> false
        | _ when key.Length > clientData.GetServerInfo.MaxKeyLength -> false
        | _ when not (stringIsOnlyAlphaNumericExcept key invalidTagKeyCharacters) -> false
        | _ -> true

    let private invalidTagValueCharacters = [| crCharacter; lfCharacter; ';'; ' '; nulCharacter; bellCharacter; spaceCharacter |]

    /// <summary>
    /// Validates the value of a tag
    /// </summary>
    /// <returns> true if it was valid </returns>
    let validateTagValue (clientData: IRCClient) (value: string) =
        match value with
        | _ when value.Length > clientData.GetServerInfo.MaxKeyLength -> false
        | _ when not (stringDoesNotContain value invalidTagValueCharacters) -> false
        | _ -> true

    /// <summary>
    /// Validate length of message string
    /// </summary>
    /// <returns> true if it was valid </returns>
    let validateMessageString (clientData: IRCClient) (message: string) =
        match message with
        | _ when message.Length > clientData.GetServerInfo.LineLength -> false
        | _ -> true

    /// <summary>
    /// Validates the topic string
    /// </summary>
    /// <returns> true if it was valid </returns>
    let validateTopic (clientData: IRCClient) (topic: string) =
        if      topic = "" then false
        else topic.Length <= clientData.GetServerInfo.MaxTopicLength

    /// <summary>
    /// Validates the channel string
    /// </summary>
    /// <returns> true if it was valid </returns>
    let validateChannel (clientData: IRCClient) (channel: string) =
        if      channel = "" then false
        else if channel.Length > clientData.GetServerInfo.MaxChannelLength then false
        else

        let channelPrefix = channel.[0]

        Map.containsKey channelPrefix clientData.GetServerInfo.ChannelPrefixes

    /// <summary>
    /// Validates a list of channels in a comma separated string
    /// </summary>
    /// <returns> true if it was valid </returns>
    let validateChannelsString (clientData: IRCClient) (channelsString: string) =
        let channelsSplit = channelsString.Split(',')

        match channelsSplit.Length with
        | length when length > clientData.GetServerInfo.MaxTargets -> false
        | 1 -> validateChannel clientData channelsString
        | _ -> 
            channelsSplit
            |> Array.forall
                ( fun ch -> validateChannel clientData ch )

    /// <summary>
    /// Validates a string of comma separated nicks
    /// </summary>
    /// <returns> true if it was valid </returns>
    let validateNicksString (clientData: IRCClient) (nicksString: string) =
        let nicksSplit = nicksString.Split(',')

        match nicksSplit.Length with
        | length when length > clientData.GetServerInfo.MaxTargets -> false
        | 1 -> validateNick clientData nicksString
        | _ -> 
            nicksSplit
            |> Array.forall
                ( fun nick -> validateNick clientData nick )
        