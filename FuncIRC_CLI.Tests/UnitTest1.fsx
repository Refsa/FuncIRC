namespace FuncIRC_CLI.Tests

#r "../FuncIRC_CLI/bin/Debug/netcoreapp2.1/FuncIRC_CLI.dll"

open NUnit.Framework
open System
open FuncIRC_CLI
open FuncIRC_CLI.CLIElement
open FuncIRC_CLI.ConsoleHelpers

module CLIElementTests =
    [<Test>]
    let ``CLIElements content is the same as when created``() =
        let content = "Test"
        let cliElement = CLIElement (content, CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black), false)
    
        Assert.AreEqual (cliElement.GetContent, content)