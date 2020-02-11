namespace FuncIRC

/// General helpers for regex
module RegexHelpers =
    open System.Text.RegularExpressions

    /// <summary>
    /// Gives all string content of matches as an array if there were any
    /// </summary>
    let matchRegex input pattern =
        let matches = Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m.Value ] else None

    /// <summary>
    /// Gives all matches as an array if there were any
    /// </summary>
    let matchRegexGroup input pattern =
        let matches = Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m ] else None

    /// <summary>
    /// Gets the string value of the first capture group
    /// </summary>
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

    /// <summary>
    /// Checks if the pattern was matched
    /// </summary>
    /// <returns> true if a match was found </returns>
    let (|RegexFound|) pattern input =
        let matches = Regex.Matches(input, pattern)
        matches.Count > 0

    /// <summary>
    /// Gives all string content of matches as an array if there were any
    /// </summary>
    let (|RegexSplit|_|) pattern input =
        let matches = Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m.Value ] else None

    /// <summary>
    /// Returns the first regex group found if there were any
    /// </summary>
    let (|RegexFirst|_|) pattern input = 
        let rmatch = matchRegex input pattern
        match rmatch with
        | None -> None
        | Some rm -> Some rm.[0]