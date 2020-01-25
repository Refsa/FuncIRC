#r "../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"

#load "../.paket/load/netstandard2.0/NUnit.fsx"
#load "TestMessages.fsx"

namespace FuncIRC.Tests

open NUnit.Framework

module MessageParserTest =
    let testIRCServerAddress = "testnet.inspircd.org"
    let testIRCServerChannel = "#refsa"
    let testIRCServerNick = "testbot"