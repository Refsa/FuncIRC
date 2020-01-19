#load "../Utils/ConsoleHelpers.fsx"
#load "CLIView.fsx"
#load "../Update/Navigation.fsx"

namespace FuncIRC_CLI

module ConsoleView =
    open ConsoleHelpers
    open CLIView
    open Navigation

    type ConsoleView =
        {
            Name: string
            CLIView: CLIView
            Navigation: Navigation
        }