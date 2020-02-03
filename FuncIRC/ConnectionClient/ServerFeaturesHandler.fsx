#load "IRCClientData.fsx"
#load "IRCInformation.fsx"
#load "../Utils/GeneralHelpers.fsx"

namespace FuncIRC

open IRCClientData
open IRCInformation
open GeneralHelpers

#if !DEBUG
module internal ServerFeaturesHandler =
#else
module ServerFeaturesHandler =
#endif

    /// NETWORK
    let networkFeatureHandler (networkFeature, clientData: IRCClientData) =
        clientData.ServerInfo <- {clientData.ServerInfo with Name = networkFeature}

    /// CASEMAPPING
    let casemappingFeatureHandler (casemappingFeature, clientData: IRCClientData) =
        let casemapping =        
            match casemappingFeature with
            | "ascii"          -> Casemapping.ASCII
            | "rfc1459"        -> Casemapping.RFC1459
            | "rfc1459-strict" -> Casemapping.RFC1459Strict
            | "rfc7613"        -> Casemapping.RFC7613
            | _                -> Casemapping.Unknown

        clientData.ServerInfo <- {clientData.ServerInfo with Casemapping = casemapping}

    /// MAXTARGETS
    let maxTargetsFeatureHandler (maxTargetsFeature, clientData: IRCClientData) =
        match maxTargetsFeature with
        | IntParsed value -> clientData.ServerInfo <- {clientData.ServerInfo with MaxTargets = value}
        | InvalidParse -> ()

    /// CHANTYPES    
    let chanTypesFeatureHandler (chanTypesFeature: string, clientData: IRCClientData) =
        let supportedChanTypes =
            [|
                for c in chanTypesFeature -> (c, 10)
            |] |> Map.ofArray

        clientData.ServerInfo <- {clientData.ServerInfo with ChannelPrefixes = supportedChanTypes}

    /// CHANLIMIT    
    let chanLimitFeatureHandler (chanLimitFeature: string, clientData: IRCClientData) = 
        let channels = chanLimitFeature.Split(';')

        let chanLimits =
            [|
                for chan in channels ->
                    let kvp = chan.Split(':')
                    (char kvp.[0], int kvp.[1])
            |] |> Map.ofArray

        clientData.ServerInfo <- {clientData.ServerInfo with ChannelPrefixes = chanLimits}

    /// CHANMODES
    let chanModesFeatureHandler (chanModesFeature, clientData: IRCClientData) = ()

    /// LINELEN
    let linelengthFeatureHandler (linelenFeature, clientData: IRCClientData) =
        match linelenFeature with
        | IntParsed value -> clientData.ServerInfo <- {clientData.ServerInfo with LineLength = value}
        | InvalidParse -> ()

    /// CHANNELLEN
    let chanLengthFeatureHandler (chanLenFeature, clientData: IRCClientData) =
        match chanLenFeature with
        | IntParsed value -> clientData.ServerInfo <- {clientData.ServerInfo with MaxChannelLength = value}
        | InvalidParse -> ()

    /// AWAYLEN
    let awayLengthHandler (feature, clientData: IRCClientData) =
        match feature with
        | IntParsed value -> clientData.ServerInfo <- {clientData.ServerInfo with MaxAwayLength = value}
        | InvalidParse -> ()

    /// KICKLEN    
    let kickLengthHandler (feature, clientData: IRCClientData) =
        match feature with
        | IntParsed value -> clientData.ServerInfo <- {clientData.ServerInfo with MaxKickLength = value}
        | InvalidParse -> ()

    /// TOPICLEN
    let topicLengthHandler (feature, clientData: IRCClientData) =
        match feature with
        | IntParsed value -> clientData.ServerInfo <- {clientData.ServerInfo with MaxTopicLength = value}
        | InvalidParse -> ()

    /// USERLEN
    let userLengthHandler (feature, clientData: IRCClientData) =
        match feature with
        | IntParsed value -> clientData.ServerInfo <- {clientData.ServerInfo with MaxUserLength = value}
        | InvalidParse -> ()

    /// NICKLEN
    let nickLengthHandler (feature, clientData: IRCClientData) =
        match feature with
        | IntParsed value -> clientData.ServerInfo <- {clientData.ServerInfo with MaxNickLength = value}
        | InvalidParse -> ()

    /// MODES
    let modesHandler (feature, clientData: IRCClientData) =
        match feature with
        | IntParsed value -> clientData.ServerInfo <- {clientData.ServerInfo with MaxModes = value}
        | InvalidParse -> ()

    /// KEYLEN
    let keyLengthHandler (feature, clientData: IRCClientData) =
        match feature with
        | IntParsed value -> clientData.ServerInfo <- {clientData.ServerInfo with MaxKeyLength = value}
        | InvalidParse -> ()

    /// HOSTLEN
    let hostLengthHandler (feature, clientData: IRCClientData) =
        match feature with
        | IntParsed value -> clientData.ServerInfo <- {clientData.ServerInfo with MaxHostLength = value}
        | InvalidParse -> ()

    /// Empty handler
    let noFeatureHandler (noFeature, clientData: IRCClientData) =
        ()

    let serverFeaturesHandler (features: (string * string) array, clientData: IRCClientData) =
        features
        |> Array.iter
            (fun ai ->
                let k, v = ai
                (v, clientData) |>
                (
                    match k with
                    | "NETWORK"     -> networkFeatureHandler
                    | "CASEMAPPING" -> casemappingFeatureHandler
                    | "LINELEN"     -> linelengthFeatureHandler
                    | "MAXTARGETS"  -> maxTargetsFeatureHandler
                    | "CHANTYPES"   -> chanTypesFeatureHandler
                    | "CHANLIMIT"   -> chanLimitFeatureHandler
                    | "CHANMODES"   -> chanModesFeatureHandler
                    | "CHANNELLEN"  -> chanLengthFeatureHandler
                    | "AWAYLEN"     -> awayLengthHandler
                    | "KICKLEN"     -> kickLengthHandler
                    | "TOPICLEN"    -> topicLengthHandler
                    | "USERLEN"     -> userLengthHandler
                    | "NICKLEN"     -> nickLengthHandler
                    | "MODES"       -> modesHandler
                    | "KEYLEN"      -> keyLengthHandler
                    | "HOSTLEN"     -> hostLengthHandler
                    | _             -> noFeatureHandler
                )
            )