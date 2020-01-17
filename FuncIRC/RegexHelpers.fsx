namespace FuncIRC

module RegexHelpers =

    let (|RegexSplit|_|) pattern input =
        let matches = System.Text.RegularExpressions.Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m.Value ] else None

    let matchRegex input pattern =
        let matches = System.Text.RegularExpressions.Regex.Matches(input, pattern)
        if matches.Count > 0 then Some [ for m in matches -> m.Value ] else None
