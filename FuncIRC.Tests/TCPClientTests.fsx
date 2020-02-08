#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"
#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests
open System.Text

module TCPClientTests =
    open NUnit.Framework
    open FuncIRC.StringHelpers
    open FuncIRC.MessageTypes

    [<Test>]
    let ``Check that TCPClient can establish TCP connection``() =
        //startClient()
        Assert.True (true)

        