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
    // validateChannel tests
    [<Test>]
    let ``validateChannel should correctly validate a channel string based on information in IRCClientData``() =
        let clientData = IRCClientData()
        let feature = [| ("CHANTYPES", "#") |]
        serverFeaturesHandler (feature, clientData)

        let validChannel1 = "#channel"
        let validChannel2 = "#otherchannel"

        let invalidChannel1 = "invalidChannel"
        let invalidChannel2 = "@invalidChannel"
        let invalidChannel3 = ""
        let invalidChannel4 = createString (clientData.GetServerInfo.MaxChannelLength + 1)

        (validateChannel clientData validChannel1) |> AssertTrue <| "validChannel1 didnt validate channel"
        (validateChannel clientData validChannel2) |> AssertTrue <| "validChannel2 didnt validate channel"

        (validateChannel clientData invalidChannel1) |> AssertFalse <| "invalidChannel1 didnt catch without prefix"
        (validateChannel clientData invalidChannel2) |> AssertFalse <| "invalidChannel2 didnt catch invalid prefix"
        (validateChannel clientData invalidChannel3) |> AssertFalse <| "invalidChannel3 didnt catch empty channeø"
        (validateChannel clientData invalidChannel4) |> AssertFalse <| "invalidChannel4 didnt check length"

    // validateNick tests
    [<Test>]
    let ``validateNick should validate nick string based on information in IRCClientData``() =
        let clientData = IRCClientData()

        let validNick1 = "somenick"
        let validNick2 = "someOtherNick"
        let validNick3 = createString (clientData.GetServerInfo.MaxNickLength)

        let invalidNick1 = "some nick"
        let invalidNick2 = createString (clientData.GetServerInfo.MaxNickLength + 1)
        let invalidNick3 = ""

        (validateNick clientData validNick1) |> AssertTrue <| "validNick1 didnt validate nick"
        (validateNick clientData validNick2) |> AssertTrue <| "validNick2 didnt validate nick"
        (validateNick clientData validNick3) |> AssertTrue <| "validNick3 didnt validate nick"

        (validateNick clientData invalidNick1) |> AssertFalse <| "invalidNick1 didnt catch nick with space"
        (validateNick clientData invalidNick2) |> AssertFalse <| "invalidNick2 didnt catch too long nick"
        (validateNick clientData invalidNick3) |> AssertFalse <| "invalidNick3 didnt catch empty nick"

    // validateUser tests
    [<Test>]
    let ``validateUser should validate user string based on information in IRCClientData``() =
        let clientData = IRCClientData()

        let validUser1 = "someUser"
        let validUser2 = "otherUser"
        let validUser3 = createString (clientData.GetServerInfo.MaxUserLength)

        let invalidUser1 = "some user"
        let invalidUser2 = createString (clientData.GetServerInfo.MaxUserLength + 1)
        let invalidUser3 = ""

        (validateUser clientData validUser1) |> AssertTrue <| "validUser1 didnt validate user"
        (validateUser clientData validUser2) |> AssertTrue <| "validUser2 didnt validate user"
        (validateUser clientData validUser3) |> AssertTrue <| "validUser3 didnt validate user"

        (validateUser clientData invalidUser1) |> AssertFalse <| "invalidUser1 didnt catch user with space"
        (validateUser clientData invalidUser2) |> AssertFalse <| "invalidUser2 didnt catch too long user length"
        (validateUser clientData invalidUser3) |> AssertFalse <| "invalidUser3 didnt catch empty user"

    // validateTopic tests
    [<Test>]
    let ``validateTopic should validate topic string based on information in IRCClientData``() =
        let clientData = IRCClientData()

        let validTopic1 = "some topic"
        let validTopic2 = "some other topic"
        let validTopic3 = createString (clientData.GetServerInfo.MaxTopicLength)

        let invalidTopic1 = ""
        let invalidTopic2 = createString (clientData.GetServerInfo.MaxTopicLength + 1)

        (validateTopic clientData validTopic1) |> AssertTrue <| "validTopic1 wasn't validated"
        (validateTopic clientData validTopic2) |> AssertTrue <| "validTopic2 wasn't validated"
        (validateTopic clientData validTopic3) |> AssertTrue <| "validTopic3 wasn't validated"

        (validateTopic clientData invalidTopic1) |> AssertFalse <| "invalidTopic1 didnt catch empty topic"
        (validateTopic clientData invalidTopic2) |> AssertFalse <| "invalidTopic2 didnt catch too long topic length"