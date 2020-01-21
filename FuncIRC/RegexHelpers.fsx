namespace FuncIRC

/// General helpers for regex
module RegexHelpers =
    /// Gives all matches as an array if there were any
    let (|RegexSplit|_|) pattern input =
        let matches = System.Text.RegularExpressions.Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m.Value ] else None

    /// Gives all matches as an array if there were any
    let matchRegex input pattern =
        let matches = System.Text.RegularExpressions.Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m.Value ] else None
