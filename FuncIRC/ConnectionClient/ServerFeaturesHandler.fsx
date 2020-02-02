#load "IRCClientData.fsx"

namespace FuncIRC

open IRCClientData

module internal ServerFeaturesHandler =
    let serverFeaturesHandler (features: string * string array, clientData: IRCClientData) =
        ()