#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"
#load "NUnitTestHelpers.fsx"
#load "TestMessages.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework
open NUnitTestHelpers

open TestMessages

open FuncIRC.IRCMessages
open FuncIRC.IRCClientData
open FuncIRC.ServerFeaturesHandler
open FuncIRC.Validators

module ValidatorsTests =
    /// Creates a IRCClientData object with the given comma separated channel prefixes added
    let mockIRCClientDataWithChannelPrefix (channelPrefixes: string) =
        let clientData = IRCClientData()
        let feature = [| ("CHANTYPES", channelPrefixes) |]
        serverFeaturesHandler (feature, clientData)
        clientData

    // validateChannel tests
    [<Test>]
    let ``validateChannel should correctly validate a channel string based on information in IRCClientData``() =
        let clientData = mockIRCClientDataWithChannelPrefix "#"

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

    /// validateHostname tests
    [<Test>]
    let ``validateHostname should filter out invalid hostnames``() =
        let clientData = IRCClientData()

        hostnameTests
        |> List.iter
            (fun hn ->
                let result = (validateHostname clientData hn.Hostname) = hn.Valid

                if not result then
                    printfn "Hostname %s was supposed to be %b" hn.Hostname hn.Valid

                Assert.True(result)
            )

    /// validateSource tests
    [<Test>]
    let ``validateSource should validate Source record based on information in IRCClientData``()=
        let clientData = IRCClientData()

        sourceTests
        |> List.iter
            (fun st ->
                let result = (validateSource clientData st.Source) = st.Valid

                if not result then
                    printfn "Source %s was supposed to be %b" (st.Source.ToString) st.Valid

                Assert.True(result)
            )

    /// validateTagKey tests
    [<Test>]
    let ``validateTagKey should validate keys of tags in a message``() =
        let clientData = IRCClientData()

        // Valid tests
        testMessages
        |> List.iter
            (fun tm ->
                match tm.Output.Tags with
                | Some tags -> 
                    tags
                    |> List.iter
                        (fun tag ->
                            (validateTagKey clientData tag.Key) |> AssertTrue <| ("Tag key " + tag.Key + " should be valid")
                        )
                | None -> ()
            )

        // Invalid tests
        invalidTags
        |> List.iter
            (fun tag ->
                (validateTagKey clientData tag.Key) |> AssertFalse <| ("Tag key " + tag.Key + " should be invalid")
            )

        // Key length tests
        let validLongKey = createString (clientData.GetServerInfo.MaxKeyLength)
        let invalidLongKey = createString (clientData.GetServerInfo.MaxKeyLength + 1)

        (validateTagKey clientData validLongKey) |> AssertTrue <| "Long key within ServerInfo.MaxKeyLength of Tag should be valid"
        (validateTagKey clientData invalidLongKey) |> AssertFalse <| "Key longer than ServerInfo.MaxKeyLength of Tag should be invalid"

    /// validateTagValue tests
    [<Test>]
    let ``validateTagValue should validate values of Tags in a message``() =
        let clientData = IRCClientData()

        // Valid tests
        testMessages
        |> List.iter
            (fun tm ->
                match tm.Output.Tags with
                | Some tags -> 
                    tags
                    |> List.iter
                        (fun tag ->
                            match tag.Value with
                            | Some value ->
                                (validateTagValue clientData value) |> AssertTrue <| ("Tag Value " + value + " should be valid")
                            | None -> ()
                        )
                | None -> ()
            )

        // Invalid tests
        invalidTags
        |> List.iter
            (fun tag ->
                match tag.Value with
                | Some value ->
                    (validateTagValue clientData value) |> AssertFalse <| ("Tag Value " + value + " should be invalid")
                | None -> ()
            )

        // Value length tests
        let validLongValue = createString (clientData.GetServerInfo.MaxKeyLength)
        let invalidLongValue = createString (clientData.GetServerInfo.MaxKeyLength + 1)

        (validateTagValue clientData validLongValue) |> AssertTrue <| "Long Value of Tag within ServerInfo.MaxKeyLength should be valid"
        (validateTagValue clientData invalidLongValue) |> AssertFalse <| "Value of Tag longer than ServerInfo.MaxKeyLength should be invalid"

    /// validateMessageString tests
    [<Test>]
    let ``validateMessageString should check if length of string is within line length of IRCServerInfo``() =
        let clientData = IRCClientData()

        let validMessageString = createString clientData.GetServerInfo.LineLength
        let invalidMessageString = createString (clientData.GetServerInfo.LineLength + 1)

        (validateMessageString clientData validMessageString) |> AssertTrue <| "Message string should be valid under the line length limit"
        (validateMessageString clientData invalidMessageString) |> AssertFalse <| "Message string should be invalid above the line length limit"


    /// validateListMessage tests
    [<Test>]
    let ``validateListMessage should check how many channels were requested information from``() =
        let clientData = mockIRCClientDataWithChannelPrefix "#"

        let validListMessage1 = "#channel1"
        let validListMessage2 = "#channel1,#channel2,#channel3"

        let invalidListMessage1 = "#channel0,#channel1,#channel2,#channel3,#channel4,#channel5,#channel1,#channel2,#channel3,#channel4,#channel5,#channel1,#channel2,#channel3,#channel4,#channel5,#channel1,#channel2,#channel3,#channel4,#channel5"
        
        (validateChannelsString clientData validListMessage1) |> AssertTrue <| "validListMessage1 should be validated successfully"
        (validateChannelsString clientData validListMessage2) |> AssertTrue <| "validListMessage2 should be validated successfully"

        (validateChannelsString clientData invalidListMessage1) |> AssertFalse <| "invalidListMessage1 should not be validated"

    [<Test>]
    let ``validateListMessage should check the channel prefix of each channel given``() =
        let clientData = mockIRCClientDataWithChannelPrefix "#"

        let invalidListMessage1 = "@channel1,#channel2"
        let invalidListMessage2 = "#channel1,@channel2"

        (validateChannelsString clientData invalidListMessage1) |> AssertFalse <| "invalidListMessage1 should not be validated"
        (validateChannelsString clientData invalidListMessage2) |> AssertFalse <| "invalidListMessage2 should not be validated"
