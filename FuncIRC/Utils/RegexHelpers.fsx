namespace FuncIRC

/// General helpers for regex
module RegexHelpers =
    open System.Text.RegularExpressions

    /// Gives all matches as an array if there were any
    let matchRegex input pattern =
        let matches = Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m.Value ] else None

    let matchRegexGroup input pattern =
        let matches = Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m ] else None

    let matchRegexFirst input pattern =
        let rmatch = matchRegex input pattern
        match rmatch with
        | Some rm -> Some rm.[0]
        | None -> None

    /// Currently unsupported Active Pattern
    //let (|RegexGrab|RegexNotFound|) input pattern =
    //    let matches = matchRegex input pattern
    //    match matches with
    //    | Some m -> RegexGrab m
    //    | None -> RegexNotFound

    let (|RegexFound|) pattern input =
        let matches = Regex.Matches(input, pattern)
        matches.Count > 0

    /// Gives all matches as an array if there were any
    let (|RegexSplit|_|) pattern input =
        let matches = Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m.Value ] else None

    /// Returns the first regex group found if there were any
    let (|RegexFirst|_|) pattern input = 
        let rmatch = matchRegex input pattern
        match rmatch with
        | None -> None
        | Some rm -> Some rm.[0]