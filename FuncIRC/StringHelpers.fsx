namespace FuncIRC

module StringHelpers=
    open System

    let stringTrimFirstIf (target:string, character: char): string =
        if target.[0] = character then
            target.[1..target.Length - 1]
        else
            target

    let stringTrimLastIf (target:string, character: char): string =
        if target.[target.Length - 1] = character then
            target.[0..target.Length - 2]
        else
            target