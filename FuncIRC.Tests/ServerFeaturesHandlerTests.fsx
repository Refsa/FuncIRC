#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework
open FuncIRC.ServerFeaturesHandler
open FuncIRC.IRCClientData
open FuncIRC.IRCInformation

module ServerFeaturesHandlerTests =
    let serverFeatures =
        [| 
            ("AWAYLEN", "200"); ("CASEMAPPING", "ascii"); ("CHANLIMIT", "#:20"); 
            ("CHANMODES", "b,k,l,imnpst"); ("CHANNELLEN", "64"); ("CHANTYPES", "#"); 
            ("ELIST", "CMNTU"); ("HOSTLEN", "64"); ("KEYLEN", "32"); ("KICKLEN", "255"); 
            ("LINELEN", "512"); ("MAXLIST", "b:100"); ("MAXTARGETS", "20"); ("MODES", "20"); 
            ("NETWORK", "Refsa"); ("NICKLEN", "30"); ("PREFIX", "(ov)@+"); ("SAFELIST", ""); 
            ("STATUSMSG", "@+"); ("TOPICLEN", "307"); ("USERLEN", "10"); ("WHOX", "") 
        |]

    [<Test>]
    let ``NETWORK feature should set the name of the network in IRCClientData``() =
        let clientData = IRCClientData()
        let feature = [| ("NETWORK", "SomeNetwork") |]

        serverFeaturesHandler (feature, clientData)
        let networkName = clientData.GetServerInfo.Name

        Assert.AreNotEqual ("DEFAULT", networkName)
        Assert.AreEqual ("SomeNetwork", networkName)

    let checkCasemappingInClientData casemapping (clientData: IRCClientData) =
        clientData.GetServerInfo.Casemapping = casemapping

    [<Test>]
    let ``CASEMAPPING feature should set casemapping in IRCClientData``() =
        let clientData = IRCClientData()
        let feature = [| ("CASEMAPPING", "ascii") |]
        serverFeaturesHandler (feature, clientData)
        Assert.True (checkCasemappingInClientData Casemapping.ASCII clientData, "Casemapping was not set correctly for ascii")

        let feature = [| ("CASEMAPPING", "rfc1459") |]
        serverFeaturesHandler (feature, clientData)
        Assert.True (checkCasemappingInClientData Casemapping.RFC1459 clientData, "Casemapping was not set correctly for rfc1459")

    [<Test>]
    let ``CHANTYPES feature should destruct incoming string into a char array and set ChannelPrefixes in IRCClientData``() =
        let clientData = IRCClientData()
        let feature = [| ("CHANTYPES", "#%@") |]

        serverFeaturesHandler (feature, clientData)
        [| '#'; '%'; '@' |]
        |> Array.iter
            (fun x -> clientData.GetServerInfo.ChannelPrefixes.ContainsKey x |> Assert.True)

        //Assert.AreEqual ( [| '#'; '%'; '@' |], clientData.GetServerInfo.ChannelPrefixes, "Channel Types was not parsed correctly" )

    [<Test>]
    let ``CHANLIMIT should set the limit to each channel type in IRCClientData info``() =
        let clientData = IRCClientData()
        let limitsFeature = [| ("CHANLIMIT", "#:20;%:10;@:5") |]

        serverFeaturesHandler (limitsFeature, clientData)

        [| ('#', 20); ('%', 10); ('@', 5) |]
        |> Array.iter
            (fun x ->
                let k, v = x
                clientData.GetServerInfo.ChannelPrefixes.[k] = v |> Assert.True)

    [<Test>]
    let ``CHANLIMIT should not override channel types received from CHANTYPES``() =
        let clientData = IRCClientData()
        let typesFeature = [| ("CHANTYPES", "#%@") |]
        let limitsFeature = [| ("CHANLIMIT", "#:20;%:10") |]

        serverFeaturesHandler (typesFeature, clientData)
        serverFeaturesHandler (limitsFeature, clientData)

        [| '#'; '%'; '@' |]
        |> Array.iter
            (fun x -> clientData.GetServerInfo.ChannelPrefixes.ContainsKey x |> Assert.True)

        [| ('#', 20); ('%', 10); ('@', 10) |]
        |> Array.iter
            (fun x ->
                let k, v = x
                clientData.GetServerInfo.ChannelPrefixes.[k] = v |> Assert.True)

    [<Test>]
    let ``CHANLEN should set max channel length in IRCClientData if given a valid int as string``() =
        let clientData = IRCClientData()
        let validFeature = [| ("CHANNELLEN", "64") |]
        let invalidFeature = [| ("CHANNELLEN", "abcd") |]

        serverFeaturesHandler (validFeature, clientData)
        Assert.AreEqual (64, clientData.GetServerInfo.MaxChannelLength)

        serverFeaturesHandler (invalidFeature, clientData)
        Assert.AreEqual (64, clientData.GetServerInfo.MaxChannelLength)

    [<Test>]
    let ``LINELEN feature should only accept values that can be parsed to int if not default to 512``() =
        let clientData = IRCClientData()
        let feature = [| ("LINELEN", "abcd") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (512, clientData.GetServerInfo.LineLength, "Line length set an undefined value")

    [<Test>]
    let ``LINELEN feature should parse strings to int if the value is an int``() =
        let clientData = IRCClientData()
        let feature = [| ("LINELEN", "1024") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (1024, clientData.GetServerInfo.LineLength, "LineLength was not set correctly")

    [<Test>]
    let ``CHANNELLEN feature should parse strings to int if the value is an int``() =
        let clientData = IRCClientData()
        let feature = [| ("CHANNELLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxChannelLength, "CHANNELLEN was not set correctly")

    [<Test>]
    let ``AWAYLEN feature should parse strings to int if the value is an int``() =
        let clientData = IRCClientData()
        let feature = [| ("AWAYLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxAwayLength, "AWAYLEN was not set correctly")

    [<Test>]
    let ``KICKLEN feature should parse strings to int if the value is an int``() =
        let clientData = IRCClientData()
        let feature = [| ("KICKLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxKickLength, "KICKLEN was not set correctly")

    [<Test>]
    let ``TOPICLEN feature should parse strings to int if the value is an int``() =
        let clientData = IRCClientData()
        let feature = [| ("TOPICLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxTopicLength, "TOPICLEN was not set correctly")

    [<Test>]
    let ``USERLEN feature should parse strings to int if the value is an int``() =
        let clientData = IRCClientData()
        let feature = [| ("USERLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxUserLength, "USERLEN was not set correctly")

    [<Test>]
    let ``NICKLEN feature should parse strings to int if the value is an int``() =
        let clientData = IRCClientData()
        let feature = [| ("NICKLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxNickLength, "NICKLEN was not set correctly")

    [<Test>]
    let ``MODES feature should parse strings to int if the value is an int``() =
        let clientData = IRCClientData()
        let feature = [| ("MODES", "1000") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxModes, "MODES was not set correctly")

    [<Test>]
    let ``KEYLEN feature should parse strings to int if the value is an int``() =
        let clientData = IRCClientData()
        let feature = [| ("KEYLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxKeyLength, "KEYLEN was not set correctly")

    [<Test>]
    let ``HOSTLEN feature should parse strings to int if the value is an int``() =
        let clientData = IRCClientData()
        let feature = [| ("HOSTLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxHostLength, "HOSTLEN was not set correctly")

    [<Test>]
    let ``MAXTARGETS feature should parse strings to int if the value is an int``() =
        let clientData = IRCClientData()
        let feature = [| ("MAXTARGETS", "1000") |]
        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxTargets, "MAXTARGETS was not set correctly")