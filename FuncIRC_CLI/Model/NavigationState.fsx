#load "../View/CLIElement.fsx"

namespace FuncIRC_CLI

module NavigationState =
    open CLIElement

    type NavigationState =
        {
            Focused: CLIElement
            Index: int
        }