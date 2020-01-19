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

    let setupLoginView(viewSize): ConsoleView =
        let defaultColor = CLIColor (ConsoleColor.Green, ConsoleColor.Black)
        let inverseColor = CLIColor (ConsoleColor.Black, ConsoleColor.White)

        // Elements
        let titleString = "---~~~~{### FuncIRC CLI ###}~~~~---"
        let titleElement = Label(titleString, CLIPosition(viewSize.Width / 2 - titleString.Length / 2, 1), inverseColor)

        let usernameString = "Username: "
        let usernameElement = TextField(usernameString, CLIPosition(20, 4), defaultColor)

        let passwordString = "Password: "
        let passwordElement = TextField(passwordString, CLIPosition(20, 5), defaultColor)

        let logElement = TextField ("Log: ", CLIPosition (5, viewSize.Height - 3), defaultColor)

        let loginString = "[Login]"
        let loginElement = Button(loginString, CLIPosition(20, 7), defaultColor)

        let exitString = "[Exit]"
        let exitElement = Button(exitString, CLIPosition(5, viewSize.Height - 1), defaultColor)

        // View and Navigation
        let loginView = CLIView (viewSize.Height, viewSize.Width)
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
        loginView.SetLine ({Content = (buildString "=" viewSize.Width);
                              ForegroundColor = ConsoleColor.Red;
                              BackgroundColor = ConsoleColor.Black}, 0)

        loginView.SetLine ({Content = (buildString "=" viewSize.Width);
                              ForegroundColor = ConsoleColor.Red;
                              BackgroundColor = ConsoleColor.Black}, 2)

        loginView.SetLine ({Content = (buildString "¨" viewSize.Width);
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, viewSize.Height)
        // ## Title Bar End

        {
            Name = "LoginView"
            CLIView = loginView
            Navigation = loginNavigation
        }
