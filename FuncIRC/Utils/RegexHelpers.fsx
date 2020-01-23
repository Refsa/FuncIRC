namespace FuncIRC

/// General helpers for regex
module RegexHelpers =
    /// Gives all matches as an array if there were any
    let matchRegex input pattern =
        let matches = System.Text.RegularExpressions.Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m.Value ] else None

    let matchRegexFirst input pattern =
        let rmatch = matchRegex input pattern
        match rmatch with
        | Some rm -> Some rm.[0]
        | None -> None

    let (|RegexFound|) pattern input =
        let matches = System.Text.RegularExpressions.Regex.Matches(input, pattern)
        matches.Count > 0

    /// Gives all matches as an array if there were any
    let (|RegexSplit|_|) pattern input =
        let matches = System.Text.RegularExpressions.Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m.Value ] else None

    /// Returns the first regex group found if there were any
    let (|RegexFirst|_|) pattern input = 
        let rmatch = matchRegex input pattern
        match rmatch with
        | None -> None
        | Some rm -> Some rm.[0]