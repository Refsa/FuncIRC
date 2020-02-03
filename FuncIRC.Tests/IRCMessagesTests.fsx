#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework

open FuncIRC.IRCMessages
open FuncIRC.IRCClientData

module IRCMessagesTests =
    
    [<Test>]
    let ``sendAwayMessage should add an out message if length is within bounds``() =
        let clientData = IRCClientData()

        let validMessage = "Away message that is not too long"
        let invalidMessage = 
            let maxLength = clientData.GetServerInfo.MaxAwayLength + 1
            let rec buildLongMessage (currentMessage: string) =
                if currentMessage.Length = maxLength then currentMessage
                else buildLongMessage (currentMessage + "_")
            buildLongMessage "Away message that is too long "

        sendAwayMessage clientData validMessage |> Assert.True

        let outboundMessage = clientData.GetOutboundMessages
        Assert.AreEqual ("AWAY " + validMessage, outboundMessage.Replace("\r\n", ""))

        sendAwayMessage clientData invalidMessage |> not |> Assert.True

        let outboundMessage = clientData.GetOutboundMessages

        Assert.AreEqual ("", outboundMessage)
