#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework
open FuncIRC.ServerFeaturesHandler
open FuncIRC.IRCClientData

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
