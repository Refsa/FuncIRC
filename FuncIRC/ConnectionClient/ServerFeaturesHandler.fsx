#load "IRCClientData.fsx"
#load "IRCInformation.fsx"

namespace FuncIRC

open IRCClientData
open IRCInformation

#if !DEBUG
module internal ServerFeaturesHandler =
#else
module ServerFeaturesHandler =
#endif

    let networkFeatureHandler (networkFeature, clientData: IRCClientData) =
        clientData.ServerInfo <- {clientData.ServerInfo with Name = networkFeature}

    let casemappingFeatureHandler (casemappingFeature, clientData: IRCClientData) =
        let casemapping =        
            match casemappingFeature with
            | "ascii"          -> Casemapping.ASCII
            | "rfc1459"        -> Casemapping.RFC1459
            | "rfc1459-strict" -> Casemapping.RFC1459Strict
            | "rfc7613"        -> Casemapping.RFC7613
            | _                -> Casemapping.Unknown

        clientData.ServerInfo <- {clientData.ServerInfo with Casemapping = casemapping}

    let linelengthFeatureHandler (linelenFeature, clientData: IRCClientData) =
        try
            let parsed = int linelenFeature
            clientData.ServerInfo <- {clientData.ServerInfo with LineLength = parsed}
        with
        | _ -> ()

    let maxTargetsFeatureHandler (maxTargetsFeature, clientData: IRCClientData) =
        ()

    let chanTypesFeatureHandler (chanTypesFeature, clientData: IRCClientData) =
        ()

    let chanLimitFeatureHandler (chanLimitFeature, clientData: IRCClientData) =
        ()

    let chanModesFeatureHandler (chanModesFeature, clientData: IRCClientData) =
        ()

    let chanLengthFeatureHandler (chanLenFeature, clientData: IRCClientData) =
        ()

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
                    | _             -> noFeatureHandler
                )
            )