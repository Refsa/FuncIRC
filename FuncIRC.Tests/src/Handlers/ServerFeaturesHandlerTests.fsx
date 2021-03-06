#r "../../../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../../../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework
open FuncIRC.ServerFeatureHandlers
open FuncIRC.IRCClient
open FuncIRC.IRCInformation

module ServerFeaturesHandlerTests =
    // NETWORK tests
    [<Test>]
    let ``NETWORK feature should set the name of the network in IRCClient``() =
        let clientData = new IRCClient()
        let feature = [| ("NETWORK", "SomeNetwork") |]

        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        let networkName = clientData.GetServerInfo.Name

        Assert.AreNotEqual ("DEFAULT", networkName)
        Assert.AreEqual ("SomeNetwork", networkName)

    // CASEMAPPING tests
    let checkCasemappingInClientData casemapping (clientData: IRCClient) =
        clientData.GetServerInfo.Casemapping = casemapping

    [<Test>]
    let ``CASEMAPPING feature should set casemapping in IRCClient``() =
        let clientData = new IRCClient()
        let feature = [| ("CASEMAPPING", "ascii") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.True (checkCasemappingInClientData Casemapping.ASCII clientData, "Casemapping was not set correctly for ascii")

        let feature = [| ("CASEMAPPING", "rfc1459") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.True (checkCasemappingInClientData Casemapping.RFC1459 clientData, "Casemapping was not set correctly for rfc1459")

    // CHANTYPES tests
    [<Test>]
    let ``CHANTYPES feature should destruct incoming string into a char array and set ChannelPrefixes in IRCClient``() =
        let clientData = new IRCClient()
        let feature = [| ("CHANTYPES", "#%@") |]

        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)

        [| '#'; '%'; '@' |]
        |> Array.iter
            (fun x -> clientData.GetServerInfo.ChannelPrefixes.ContainsKey x |> Assert.True)

    // # CHANLIMIT tests
    [<Test>]
    let ``CHANLIMIT should set the limit to each channel type in IRCClient info``() =
        let clientData = new IRCClient()
        let limitsFeature = [| ("CHANLIMIT", "#:20;%:10;@:5") |]

        serverFeaturesHandler (limitsFeature, clientData)
        System.Threading.Thread.Sleep(1000)

        [| ('#', 20); ('%', 10); ('@', 5) |]
        |> Array.iter
            (fun x ->
                let k, v = x
                clientData.GetServerInfo.ChannelPrefixes.[k] = v |> Assert.True)

    [<Test>]
    let ``CHANLIMIT should not override channel types received from CHANTYPES``() =
        let clientData = new IRCClient()
        let typesFeature = [| ("CHANTYPES", "#%@") |]
        let limitsFeature = [| ("CHANLIMIT", "#:20;%:10") |]

        serverFeaturesHandler (typesFeature, clientData)
        serverFeaturesHandler (limitsFeature, clientData)
        System.Threading.Thread.Sleep(1000)

        clientData.GetServerInfo.ChannelPrefixes
        |> Map.toList
        |> List.iter (
            fun x ->
                printfn "k: %c - v: %d" (fst x) (snd x)
        )

        [| '#'; '%'; '@' |]
        |> Array.iter
            (fun x -> clientData.GetServerInfo.ChannelPrefixes.ContainsKey x |> Assert.True)

        [| ('#', 20); ('%', 10); ('@', 10) |]
        |> Array.iter
            (fun x ->
                let k, v = x
                clientData.GetServerInfo.ChannelPrefixes.[k] = v |> Assert.True)
    // / CHANLIMIT tests

    // CHANMODES tests
    [<Test>]
    let ``CHANMODES should set the letters that correspond to each channel mode for Type A B C D``() =
        let clientData = new IRCClient()
        let chanmodesFeature = [| ("CHANMODES", "b,k,l,imnpst") |]

        serverFeaturesHandler (chanmodesFeature, clientData)
        System.Threading.Thread.Sleep(100)

        let chanModes = clientData.GetServerInfo.ChannelModes

        Assert.AreEqual ("b", chanModes.TypeA)
        Assert.AreEqual ("k", chanModes.TypeB)
        Assert.AreEqual ("l", chanModes.TypeC)
        Assert.AreEqual ("imnpst", chanModes.TypeD)

    // # PREFIX tests
    [<Test>]
    let ``PREFIX should store all the prefixes given in UserPrefix of IRCClient``() =
        // MOCK data
        let validFeature1 = [| ("PREFIX", "(ov)@+") |]
        let validFeature2 = [| ("PREFIX", "(qaohv)~&@%+") |]
        let validFeature3 = [| ("PREFIX", "") |]
        
        let wantedFeature1 = 
            { default_IRCUserModes with Operator = "@"; Voice = "+" }
        let wantedFeature2 = 
            { Founder = "~"; Protected = "&"; Operator = "@"; Halfop = "%"; Voice = "+" }
        let wantedFeature3 = default_IRCUserModes

        // Run tests
        // validFeature1
        let clientData = new IRCClient()
        serverFeaturesHandler (validFeature1, clientData)
        System.Threading.Thread.Sleep(1000)

        let userModes = clientData.GetServerInfo.UserModes
        (wantedFeature1, userModes) |> Assert.AreEqual
        
        // validFeature2
        let clientData = new IRCClient()
        serverFeaturesHandler (validFeature2, clientData)
        System.Threading.Thread.Sleep(100)

        let userModes = clientData.GetServerInfo.UserModes
        (wantedFeature2, userModes) |> Assert.AreEqual

        // validFeature3
        let clientData = new IRCClient()
        serverFeaturesHandler (validFeature3, clientData)
        System.Threading.Thread.Sleep(100)

        let userModes = clientData.GetServerInfo.UserModes
        (wantedFeature3, userModes) |> Assert.AreEqual

    [<Test>]
    let ``PREFIX handler should discard invalid arguments``() =
        let invalidFeature1 = [| ("PREFIX", "(ov)@") |]
        let clientData = new IRCClient()

        serverFeaturesHandler (invalidFeature1, clientData)
        System.Threading.Thread.Sleep(100)
        let userModes = clientData.GetServerInfo.UserModes

        (default_IRCUserModes, userModes) |> Assert.AreEqual
    // / PREFIX tests

    // # STATUSMSG tests
    [<Test>]
    let ``STATUSMSG should parse and put prefix symbols in StatusMessageModes in the IRCServerInfo record``() =
        let validFeature1 = [| ("STATUSMSG", "@+") |]
        let validFeature2 = [| ("STATUSMSG", "&@+") |]

        let wantedFeature1 = [| '@'; '+' |]
        let wantedFeature2 = [| '&'; '@'; '+' |]

        let clientData = new IRCClient()
        serverFeaturesHandler (validFeature1, clientData)
        System.Threading.Thread.Sleep(100)
        let statusMessageModes = clientData.GetServerInfo.StatusMessageModes
        (wantedFeature1, statusMessageModes) |> Assert.AreEqual

        let clientData = new IRCClient()
        serverFeaturesHandler (validFeature2, clientData)
        System.Threading.Thread.Sleep(100)
        let statusMessageModes = clientData.GetServerInfo.StatusMessageModes
        (wantedFeature2, statusMessageModes) |> Assert.AreEqual

    [<Test>]
    let ``STATUSMSG handler should not parse empty arguments``() =
        let invalidFeature = [| ("STATUSMSG", "") |]
        let wantedFeature = Array.empty

        let clientData = new IRCClient()
        serverFeaturesHandler (invalidFeature, clientData)
        System.Threading.Thread.Sleep(100)
        let statusMessageModes = clientData.GetServerInfo.StatusMessageModes
        Assert.AreEqual (wantedFeature, statusMessageModes)
    // / STATUSMSG tests

    // # MAXLIST tests
    [<Test>]
    let ``MAXLIST should set max number of type A channel modes on MaxTypeAModes in IRCServerInfo``() =
        let validFeature1 = [| ("MAXLIST", "b:") |]
        let validFeature2 = [| ("MAXLIST", "b:20,a:50") |]

        let wantedResult1 = [ ('b', 100) ] |> Map.ofList
        let wantedResult2 = [ ('b', 20); ('a', 50) ] |> Map.ofList

        let clientData = new IRCClient()
        serverFeaturesHandler (validFeature1, clientData)
        System.Threading.Thread.Sleep(100)
        let maxTypeAModes = clientData.GetServerInfo.MaxTypeAModes
        (wantedResult1, maxTypeAModes) |> Assert.AreEqual

        let clientData = new IRCClient()
        serverFeaturesHandler (validFeature2, clientData)
        System.Threading.Thread.Sleep(100)
        let maxTypeAModes = clientData.GetServerInfo.MaxTypeAModes
        (wantedResult2, maxTypeAModes) |> Assert.AreEqual

    [<Test>]
    let ``MAXLIST should do nothing with empty parameters``() =
        let validFeature1 = [| ("MAXLIST", "") |]
        let clientData = new IRCClient()
        serverFeaturesHandler (validFeature1, clientData)
        System.Threading.Thread.Sleep(100)

        (Map.empty, clientData.GetServerInfo.MaxTypeAModes) |> Assert.AreEqual
    // / MAXLIST tests

    // SAFELIST tests
    [<Test>]
    let ``SAFELIST should set the safelist flag in IRCServerInfo``() =
        let validFeature = [| ("SAFELIST", "") |]
        let clientData = new IRCClient()
        serverFeaturesHandler (validFeature, clientData)
        System.Threading.Thread.Sleep(100)

        clientData.GetServerInfo.Safelist |> Assert.True 

    // ELIST tests
    [<Test>]
    let ``ELIST should set the supported search extensions in IRCServerInfo``() =
        let validFeature = [| ("ELIST", "MNUCT") |]
        let wantedResult = [ 'M'; 'N'; 'U'; 'C'; 'T' ]

        let clientData = new IRCClient()

        serverFeaturesHandler (validFeature, clientData)
        System.Threading.Thread.Sleep(1000)
        (wantedResult, clientData.GetServerInfo.SearchExtensions) |> Assert.AreEqual

//#region int parsing tests
module IntParsingTests =
    // # LINELEN tests
    [<Test>]
    let ``LINELEN feature should only accept values that can be parsed to int if not default to 512``() =
        let clientData = new IRCClient()
        let feature = [| ("LINELEN", "abcd") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (512, clientData.GetServerInfo.LineLength, "Line length set an undefined value")

    [<Test>]
    let ``LINELEN feature should parse strings to int if the value is an int``() =
        let clientData = new IRCClient()
        let feature = [| ("LINELEN", "1024") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (1024, clientData.GetServerInfo.LineLength, "LineLength was not set correctly")
    // / LINELEN tests

    // # CHANNELLEN tests
    [<Test>]
    let ``CHANNELLEN feature should parse strings to int if the value is an int``() =
        let clientData = new IRCClient()
        let feature = [| ("CHANNELLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxChannelLength, "CHANNELLEN was not set correctly")

    [<Test>]
    let ``CHANNELLEN should set max channel length in IRCClient if given a valid int as string``() =
        let clientData = new IRCClient()
        let validFeature = [| ("CHANNELLEN", "64") |]
        let invalidFeature = [| ("CHANNELLEN", "abcd") |]

        serverFeaturesHandler (validFeature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (64, clientData.GetServerInfo.MaxChannelLength)

        serverFeaturesHandler (invalidFeature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (64, clientData.GetServerInfo.MaxChannelLength)
    // / CHANNELLEN tests

    // AWAYLEN tests
    [<Test>]
    let ``AWAYLEN feature should parse strings to int if the value is an int``() =
        let clientData = new IRCClient()
        let feature = [| ("AWAYLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(1000)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxAwayLength, "AWAYLEN was not set correctly")

    // KICKLEN tests
    [<Test>]
    let ``KICKLEN feature should parse strings to int if the value is an int``() =
        let clientData = new IRCClient()
        let feature = [| ("KICKLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxKickLength, "KICKLEN was not set correctly")

    // TOPICLEN tests
    [<Test>]
    let ``TOPICLEN feature should parse strings to int if the value is an int``() =
        let clientData = new IRCClient()
        let feature = [| ("TOPICLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxTopicLength, "TOPICLEN was not set correctly")

    // USERLEN tests
    [<Test>]
    let ``USERLEN feature should parse strings to int if the value is an int``() =
        let clientData = new IRCClient()
        let feature = [| ("USERLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxUserLength, "USERLEN was not set correctly")

    // NICKLEN tests
    [<Test>]
    let ``NICKLEN feature should parse strings to int if the value is an int``() =
        let clientData = new IRCClient()
        let feature = [| ("NICKLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxNickLength, "NICKLEN was not set correctly")

    // MODES tests
    [<Test>]
    let ``MODES feature should parse strings to int if the value is an int``() =
        let clientData = new IRCClient()
        let feature = [| ("MODES", "1000") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxModes, "MODES was not set correctly")

    // KEYLEN tests
    [<Test>]
    let ``KEYLEN feature should parse strings to int if the value is an int``() =
        let clientData = new IRCClient()
        let feature = [| ("KEYLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxKeyLength, "KEYLEN was not set correctly")

    // HOSTLEN tests
    [<Test>]
    let ``HOSTLEN feature should parse strings to int if the value is an int``() =
        let clientData = new IRCClient()
        let feature = [| ("HOSTLEN", "1000") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxHostLength, "HOSTLEN was not set correctly")

    // MAXTARGETS tests
    [<Test>]
    let ``MAXTARGETS feature should parse strings to int if the value is an int``() =
        let clientData = new IRCClient()
        let feature = [| ("MAXTARGETS", "1000") |]
        serverFeaturesHandler (feature, clientData)
        System.Threading.Thread.Sleep(100)
        Assert.AreEqual (1000, clientData.GetServerInfo.MaxTargets, "MAXTARGETS was not set correctly")
//#endregion int parsing tests