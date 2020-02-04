#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework

open FuncIRC.IRCMessages
open FuncIRC.IRCClientData
open FuncIRC.ServerFeaturesHandler

module ValidatorsTests =
    // Constructs a string of given length
    let createInvalidString maxLength =
        let rec buildLongMessage (currentMessage: string) =
            if currentMessage.Length = maxLength then currentMessage
            else buildLongMessage (currentMessage + "_")
        buildLongMessage ""

    [<Test>]
    let ``validateChannel should correctly validate a channel string based on information in IRCClientData`` =
        let clientData = IRCClientData()
        let feature = [| ("CHANTYPES", "#") |]
        serverFeaturesHandler (feature, clientData)

        let validChannel1 = "#channel"
        let validChannel2 = "#otherchannel"
        let invalidChannel1 = "invalidChannel"
        let invalidChannel2 = "@invalidChannel"

        validateChannel clientData validChannel1 |> Assert.True
        validateChannel clientData validChannel2 |> Assert.True

        validateChannel clientData invalidChannel1 |> Assert.False
        validateChannel clientData invalidChannel2 |> Assert.False

    [<Test>]
    let ``validateNick should validate nick string based on information in IRCClientData`` =
        let clientData = IRCClientData()

        let validNick1 = "somenick"
        let validNick2 = "someOtherNick"

        let invalidNick1 = "some nick"
        let invalidNick2 = createInvalidString (clientData.GetServerInfo.MaxNickLength + 1)

        validateNick clientData validNick1 |> Assert.True
        validateNick clientData validNick2 |> Assert.True

        validateNick clientData invalidNick1 |> Assert.False
        validateNick clientData invalidNick2 |> Assert.False

