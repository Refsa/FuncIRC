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
    let ``CHANTYPES feature should destruct incoming string into a char array and set ChannelPrefixes in IRCClientData``() =
        let clientData = IRCClientData()
        let feature = [| ("CHANTYPES", "#%@") |]

        serverFeaturesHandler (feature, clientData)
        Assert.AreEqual ( [| '#'; '%'; '@' |], clientData.GetServerInfo.ChannelPrefixes, "Channel Types was not parsed correctly" )

    