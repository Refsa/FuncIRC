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
        let current = clientData.ServerInfo.ChannelPrefixes |> Map.toList

        let chanLimits =
            [|
                for chan in channels ->
                    let kvp = chan.Split(':')
                    (char kvp.[0], int kvp.[1])
            |]
            |> Map.ofArray

        /// TODO: There should be a more clean way to check and add items that already existed but werent present here
        let rec buildChannelPrefixes (leftover: (char * int) list) (acc: Map<char, int>) =
            match leftover with
            | [] -> acc
            | head :: tail ->
                let k, v = head
                if not (acc.ContainsKey k) then
                    buildChannelPrefixes tail (acc |> Map.add k v)
                else
                    buildChannelPrefixes tail acc

        let chanLimits = buildChannelPrefixes current chanLimits

        clientData.ServerInfo <- {clientData.ServerInfo with ChannelPrefixes = chanLimits}

    /// CHANMODES
    let chanModesFeatureHandler (chanModesFeature: string, clientData: IRCClientData) = 
        let chanModesSplit = chanModesFeature.Split (',')
        let chanModes = 
            match chanModesSplit.Length with
            | 1 -> {default_IRCChannelModes with TypeA = chanModesSplit.[0]}
            | 2 -> {default_IRCChannelModes with TypeA = chanModesSplit.[0]; TypeB = chanModesSplit.[1]}
            | 3 -> {default_IRCChannelModes with TypeA = chanModesSplit.[0]; TypeB = chanModesSplit.[1]; TypeC = chanModesSplit.[2]}
            | 4 -> {default_IRCChannelModes with TypeA = chanModesSplit.[0]; TypeB = chanModesSplit.[1]; TypeC = chanModesSplit.[2]; TypeD = chanModesSplit.[3]}
            | _ -> default_IRCChannelModes

        clientData.ServerInfo <- {clientData.ServerInfo with ChannelModes = chanModes}

    let findUserModeInMap (userModes: Map<char, char>) (targetMode: char) =
        if userModes.ContainsKey targetMode then
            string userModes.[targetMode] 
        else ""

    /// PREFIX
    /// TODO: Remove if branches?
    let prefixHandler (prefixFeature: string, clientData: IRCClientData) = 
        if prefixFeature = "" then ()
        else

        let prefixSplit = prefixFeature.Split(')')
        let modeNames = prefixSplit.[0].[1..] |> Seq.toList
        let modeSymbols = prefixSplit.[1] |> Seq.toList

        if modeNames.Length <> modeSymbols.Length then ()
        else

        let modeMap = List.zip modeNames modeSymbols |> Map.ofList

        let userModes =
            {
                Founder   = findUserModeInMap modeMap 'q';
                Protected = findUserModeInMap modeMap 'a';
                Operator  = findUserModeInMap modeMap 'o';
                Halfop    = findUserModeInMap modeMap 'h';
                Voice     = findUserModeInMap modeMap 'v';
            }

        clientData.ServerInfo <- {clientData.ServerInfo with UserModes = userModes}

    /// STATUSMSG
    let statusMsgHandler (statusMsgFeature, clientData: IRCClientData) =
        if statusMsgFeature = "" then ()
        else

        let statusMessageModes =
            [|
                for c in statusMsgFeature -> c
            |]

        clientData.ServerInfo <- {clientData.ServerInfo with StatusMessageModes = statusMessageModes}

    /// MAXLIST
    let maxListHandler (maxListFeature: string, clientData: IRCClientData) =
        if maxListFeature = "" then ()
        else

        let maxListFeatureSplit = maxListFeature.Split(',')

        let maxList = 
            [
                for feature in maxListFeatureSplit ->
                    let featureSplit = feature.Split(':')
                    match featureSplit.Length with
                    | 1 | 2 when featureSplit.[1] = "" -> (char featureSplit.[0], 100)
                    | _ -> (char featureSplit.[0], int featureSplit.[1])
            ] |> Map.ofList

        clientData.ServerInfo <- { clientData.ServerInfo with MaxTypeAModes = maxList }

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
                    | "PREFIX"      -> prefixHandler
                    | "STATUSMSG"   -> statusMsgHandler
                    | "MAXLIST"     -> maxListHandler
                    | _             -> noFeatureHandler
                )
            )