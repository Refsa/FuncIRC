#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework

open FuncIRC.IRCMessages
open FuncIRC.IRCClientData

module ValidatorsTests =
    [<Test>]
    let ``validateChannel should correctly validate a channel string based on information in IRCClientData`` =
        let clientData = IRCClientData()

        let validChannel1 = "#channel"
        let validChannel2 = "#otherchannel"
        let invalidChannel1 = "invalidChannel"
        let invalidChannel2 = "@invalidChannel"

        validateChannel clientData validChannel1 |> Assert.True
        validateChannel clientData validChannel2 |> Assert.True

        validateChannel clientData invalidChannel1 |> Assert.False
        validateChannel clientData invalidChannel2 |> Assert.False