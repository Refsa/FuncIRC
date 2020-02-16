#r "../FuncIRC_CLI/bin/Debug/netcoreapp2.0/FuncIRC_CLI.dll"
#load "../.paket/load/netcoreapp2.0/NUnit.fsx"

namespace FuncIRC_CLI.Tests

open NUnit.Framework
open System
open FuncIRC_CLI
open FuncIRC_CLI.CLIElement
open FuncIRC_CLI.ConsoleHelpers
open FuncIRC_CLI.ApplicationState

module CLIElementTests =
    [<Test>]
    let ``CLIElements content is the same as when created``() =
        let content = "Test"
        let cliElement = CLIElement (content, CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black), false)
    
        Assert.AreEqual (cliElement.GetContent, content)

    [<Test>]
    let ``Setting Color after CLIElement is created updates the internal color``() =
        let cliElement = CLIElement ("NONE", CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black), false)
        let newColor = CLIColor (ConsoleColor.Red, ConsoleColor.Cyan)

        cliElement.SetColor newColor

        Assert.AreEqual (cliElement.GetColor, newColor)

    [<Test>]
    let ``Width is equal to content length``() =
        let contentString = "NONENONENONE"
        let cliElement = CLIElement (contentString, CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black), false)

        Assert.AreEqual (cliElement.GetWidth, contentString.Length)

    [<Test>]
    let ``GetText should be empty on the CLIElement base type``() =
        let cliElement = CLIElement ("NONE", CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black), false)
        Assert.AreEqual (cliElement.GetText, "")
        cliElement.SetContent "Other Content"
        Assert.AreEqual (cliElement.GetText, "")

    [<Test>]
    let ``SetContent on CLIElement should update the internal content field``() =
        let cliElement = CLIElement ("NONE", CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black), false)
        let newContent = "NEWCONTENT"
        cliElement.SetContent newContent
        Assert.AreEqual (cliElement.GetContent, newContent)

    [<Test>]
    let ``GetPosition and GetLine should get the x and y values of the internal position value``() =
        let position = CLIPosition (10, 20)
        let cliElement = CLIElement ("NONE", position, CLIColor (ConsoleColor.Green, ConsoleColor.Black), false)

        Assert.AreEqual (cliElement.GetPosition, position.GetPosition())
        Assert.AreEqual (cliElement.GetLine, position.GetLine())

module ButtonTests =
    [<Test>]
    let ``Should not be able to update content value on Button Element``() =
        let buttonLabel = "Button"
        let button = Button (buttonLabel, CLIPosition (10, 20), CLIColor (ConsoleColor.Green, ConsoleColor.Black))
        button.SetContent "OtherContent"

        Assert.AreEqual (button.GetContent, buttonLabel)

    [<Test>]
    let ``Check that Execute runs the assigned delegate``() =
        let button = Button ("Button", CLIPosition (10, 20), CLIColor (ConsoleColor.Green, ConsoleColor.Black))
        button.SetExecuteDelegate (fun a -> {Running = a.Running; InputState = {Line = "Delegate Worked"; Key = a.InputState.Key}})

        let testAppState = {Running = true; InputState = {Line = "NONE"; Key = ConsoleKey.NoName}}
        let feedbackAppState = button.Execute testAppState

        Assert.AreEqual (feedbackAppState.InputState.Line, "Delegate Worked")

module TextFieldTests =
    [<Test>]
    let ``Content should not change when running SetContent``() =
        let textField = TextField ("Text Field", CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black))
        textField.SetContent "New Content"

        let content = textField.GetContent.Replace("[ ", "").Replace(" ]", "").Replace(textField.PlaceholderText, "").Replace(textField.GetText, "")

        Assert.AreEqual ("Text Field", content)

    [<Test>]
    let ``GetText and Text should return the mutable string of the TextField input``() =
        let textField = TextField ("Text Field", CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black))
        textField.SetContent "Some Content"

        Assert.AreEqual (textField.GetText, "Some Content")
        Assert.AreEqual (textField.Text, "Some Content")

module PasswordFieldTests =
    [<Test>]
    let ``Text should return asterix instead of the actual password content``() =
        let passwordField = PasswordField ("Password Field", CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black))
        let testPassword = "123456"
        passwordField.SetContent testPassword

        Assert.AreEqual (passwordField.Text, "******")
        Assert.AreEqual (passwordField.Text.Length, testPassword.Length)

    [<Test>]
    let ``GetText should still return the actual value of the password``() =
        let passwordField = PasswordField ("Password Field", CLIPosition (0, 0), CLIColor (ConsoleColor.Green, ConsoleColor.Black))
        let testPassword = "123456"
        passwordField.SetContent testPassword

        Assert.AreEqual (passwordField.GetText, testPassword)