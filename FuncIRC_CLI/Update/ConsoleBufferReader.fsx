namespace FuncIRC_CLI

module ConsoleBufferReader =
    open System.IO

    type ConsoleTextReader(inStream) =
        inherit TextReader()