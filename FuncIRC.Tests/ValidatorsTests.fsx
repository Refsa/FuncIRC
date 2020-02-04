#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"
#load "NUnitTestHelpers.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework
open NUnitTestHelpers

open FuncIRC.IRCMessages
open FuncIRC.IRCClientData
open FuncIRC.ServerFeaturesHandler

module ValidatorsTests =
    [<Test>]
    let ``validateChannel should correctly validate a channel string based on information in IRCClientData``() =
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
    let ``validateNick should validate nick string based on information in IRCClientData``() =
        let clientData = IRCClientData()

        let validNick1 = "somenick"
        let validNick2 = "someOtherNick"

        let invalidNick1 = "some nick"
        let invalidNick2 = createString (clientData.GetServerInfo.MaxNickLength + 1)

        (validateNick clientData validNick1) |> AssertTrue <| "validNick1 was wrong"
        (validateNick clientData validNick2) |> AssertTrue <| "validNick2 was wrong"

        (validateNick clientData invalidNick1) |> AssertFalse <| "invalidNick1 was wrong"
        (validateNick clientData invalidNick2) |> AssertFalse <| "invalidNick2 was wrong"

    [<Test>]
    let ``validateUser should validate user string based on information in IRCClientData``() =
        let clientData = IRCClientData()

        let validUser1 = "someUser"
        let validUser2 = "otherUser"

        let invalidUser1 = "some user"
        let invalidUser2 = createString (clientData.GetServerInfo.MaxUserLength + 1)

        (validateUser clientData validUser1) |> AssertTrue <| "validUser1 was wrong"
        (validateUser clientData validUser2) |> AssertTrue <| "validUser2 was wrong"

        (validateUser clientData invalidUser1) |> AssertFalse <| "invalidUser1 was wrong"
        (validateUser clientData invalidUser2) |> AssertFalse <| "invalidUser2 was wrong"