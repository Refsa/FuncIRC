module FuncIRC_CLI

#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "IRCTestInfo.fsx"

open FuncIRC.MessageParser
open IRCTestInfo

[<EntryPoint>]
let main argv =
    printfn "----- FuncIRC CLI -----"

    messageSplit testMessage0

    0 // return an integer exit code
