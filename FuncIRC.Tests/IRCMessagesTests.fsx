#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework

open FuncIRC.IRCMessages
open FuncIRC.IRCClientData

module IRCMessagesTests =
    
    let createInvalidMessage message maxLength =
        let rec buildLongMessage (currentMessage: string) =
            if currentMessage.Length = maxLength then currentMessage
            else buildLongMessage (currentMessage + "_")
        buildLongMessage message

    [<Test>]
    let ``sendAwayMessage should add an out message if length is within bounds``() =
        let clientData = IRCClientData()

        let validMessage = "Away message that is not too long"
        let invalidMessage = createInvalidMessage "Away message that is too long " (clientData.GetServerInfo.MaxAwayLength + 1)

        sendAwayMessage clientData validMessage |> Assert.True

        let outboundMessage = clientData.GetOutboundMessages
        Assert.AreEqual ("AWAY " + validMessage, outboundMessage.Replace("\r\n", ""))

        sendAwayMessage clientData invalidMessage |> not |> Assert.True

        let outboundMessage = clientData.GetOutboundMessages

        Assert.AreEqual ("", outboundMessage)


    [<Test>]
    let ``sendQuitMessage should add an out message if length is within bounds``() =
        let clientData = IRCClientData()

        let validMessage = "Quit message that is not too long"
        let invalidMessage = createInvalidMessage "Quit message that is too long " 201

        sendQuitMessage clientData validMessage |> Assert.True

        let outboundMessage = clientData.GetOutboundMessages
        Assert.AreEqual ("QUIT " + validMessage, outboundMessage.Replace("\r\n", ""))

        sendQuitMessage clientData invalidMessage |> not |> Assert.True

        let outboundMessage = clientData.GetOutboundMessages

        Assert.AreEqual ("", outboundMessage)