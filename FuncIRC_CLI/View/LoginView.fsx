#load "ConsoleView.fsx"
#load "CLIView.fsx"
#load "CLIElement.fsx"
#load "../Update/Navigation.fsx"
#load "../Update/ButtonFunctions.fsx"
#load "../Utils/ConsoleHelpers.fsx"
#load "../Utils/GeneralHelpers.fsx"

namespace FuncIRC_CLI

module LoginView =
    open System
    open CLIView
    open CLIElement
    open ConsoleView
    open Navigation
    open ConsoleHelpers
    open GeneralHelpers
    open ButtonFunctions

    let setupLoginView(): ConsoleView =
        let loginViewSize = {Width = 128; Height = 16}

        let defaultColor = CLIColor (ConsoleColor.Green, ConsoleColor.Black)
        let inverseColor = CLIColor (ConsoleColor.Black, ConsoleColor.White)

        // Elements
        let titleString = "---~~~~{### FuncIRC CLI ###}~~~~---"
        let titleElement = Label(titleString, CLIPosition(loginViewSize.Width / 2 - titleString.Length / 2, 1), inverseColor)

        let usernameString = "Username: "
        let usernameElement = TextField(usernameString, CLIPosition(20, 4), defaultColor)

        let passwordString = "Password: "
        let passwordElement = TextField(passwordString, CLIPosition(20, 5), defaultColor)

        let logElement = TextField ("Log: ", CLIPosition (5, loginViewSize.Height - 3), defaultColor)

        let loginString = "[Login]"
        let loginElement = Button(loginString, CLIPosition(20, 7), defaultColor)

        let exitString = "[Exit]"
        let exitElement = Button(exitString, CLIPosition(5, loginViewSize.Height - 1), defaultColor)

        let defaultLine = (buildString " " loginViewSize.Width).Remove(loginViewSize.Width, 1)
                                                             .Remove(0, 1)
                                                             .Insert(0, "|")
                                                             .Insert(loginViewSize.Width, "|")

        let topLine = (buildString "¨" loginViewSize.Width)

        // View and Navigation
        let loginView = CLIView (loginViewSize.Height, loginViewSize.Width)
        loginView.SetBaseForegroundColor ConsoleColor.Green
        loginView.SetBaseBackgroundColor ConsoleColor.Black

        let loginNavigation = Navigation (defaultColor, inverseColor)

        // Delegates
        exitElement.SetExecuteDelegate exitApp

        // Add Elements to View and Navigation
        let navigationElements = 
            [
                usernameElement :> CLIElement; 
                passwordElement :> CLIElement; 
                loginElement :> CLIElement; 
                exitElement :> CLIElement
            ]

        loginView.SetElement(titleElement)
        loginView.SetElement(logElement)
        loginView.SetElements(navigationElements)

        loginNavigation.SetElements navigationElements

        // ## Title Bar
        loginView.SetLine ({Content = (buildString "=" loginViewSize.Width);
                              ForegroundColor = ConsoleColor.Red;
                              BackgroundColor = ConsoleColor.Black}, 0)

        loginView.SetLine ({Content = (buildString "=" loginViewSize.Width);
                              ForegroundColor = ConsoleColor.Red;
                              BackgroundColor = ConsoleColor.Black}, 2)

        loginView.SetLine ({Content = topLine;
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, loginViewSize.Height)
        // ## Title Bar End

        {
            Name = "LoginView"
            CLIView = loginView
            Navigation = loginNavigation
        }
