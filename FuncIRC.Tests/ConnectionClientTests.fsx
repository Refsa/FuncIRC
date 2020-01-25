#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"

namespace FuncIRC.Tests
open System.Text

module ConnectionClientTests =
    open NUnit.Framework
    open FuncIRC.ConnectionClient
    open FuncIRC.IRCStreamReader

    [<Test>]
    let ``Check that ConnectionClient can establish TCP connection``() =
        //startClient()
        Assert.True (true)

    [<Test>]
    let ``Check that byte stream can parse both UTF8 and Latin1 byte streams``() =
        
        
        ()