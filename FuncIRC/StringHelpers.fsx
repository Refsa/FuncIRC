namespace FuncIRC

module StringHelpers=
    open System

    let stringTrimFirstIf (target:string, character: char): string =
        if target.[0] = character then
            target.[1..target.Length - 1]
        else
            target