#load "ConsoleView.fsx"
#load "CLIView.fsx"
#load "CLIElement.fsx"
#load "../Update/Navigation.fsx"
#load "../Update/ButtonFunctions.fsx"
#load "../Utils/ConsoleHelpers.fsx"
#load "../Utils/GeneralHelpers.fsx"

namespace FuncIRC_CLI

module StartupView =
    open System
    open CLIView
    open CLIElement
    open ConsoleView
    open Navigation
    open ConsoleHelpers
    open GeneralHelpers
    open ButtonFunctions

    let setupStartupView (viewSize): ConsoleView =
        let defaultColor = CLIColor (ConsoleColor.Green, ConsoleColor.Black)
        let inverseColor = CLIColor (ConsoleColor.Black, ConsoleColor.White)

        let startupView = CLIView (viewSize.Height, viewSize.Width)
        startupView.SetBaseForegroundColor ConsoleColor.Green
        startupView.SetBaseBackgroundColor ConsoleColor.Black

        let startupNavigation = Navigation (defaultColor, inverseColor)

        let titleString = "---~~~~{### FuncIRC CLI ###}~~~~---"
        let titleElement = Label(titleString, CLIPosition(viewSize.Width / 2 - titleString.Length / 2, 1), inverseColor)

        let progressBar = ProgressBar("#", CLIPosition (viewSize.Width / 4, viewSize.Height / 2), defaultColor, startupView.Draw)

        startupView.AddElement (progressBar)
        startupView.AddElement (titleElement)

        // ## Title Bar
        startupView.SetLine ({Content = (buildString "=" viewSize.Width);
                              ForegroundColor = ConsoleColor.Red;
                              BackgroundColor = ConsoleColor.Black}, 0)

        startupView.SetLine ({Content = (buildString "=" viewSize.Width);
                              ForegroundColor = ConsoleColor.Red;
                              BackgroundColor = ConsoleColor.Black}, 2)

        startupView.SetLine ({Content = (buildString "¨" viewSize.Width);
                              ForegroundColor = ConsoleColor.Green;
                              BackgroundColor = ConsoleColor.Black}, viewSize.Height)
        // ## Title Bar End

        {
            Name = "Startup"
            CLIView = startupView
            Navigation = startupNavigation
        }