#load "../Utils/RegexHelpers.fsx"

namespace FuncIRC

module Validators =
    open System
    open RegexHelpers

    let validateHostname (hostname: string) =
        match hostname with
        | "" -> false
        | _ when hostname.IndexOf('.') <> -1 -> 
            match () with
            | _ when Char.IsLetterOrDigit (hostname.[0]) -> true
            | _ -> false
        |_ -> false