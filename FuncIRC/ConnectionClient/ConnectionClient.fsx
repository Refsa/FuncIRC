namespace FuncIRC

module ConnectionClient =
    open System
    open System.IO
    open System.Text
    open System.Threading
    open System.Net
    open System.Net.Sockets

    exception ClientConnectionException

    /// Wrapper for TcpClient
    type TCPClient(server: string, port: int) =
        let client: TcpClient = new TcpClient(server, port)
        let stream: NetworkStream = client.GetStream()

        member this.Connected = client.Connected
        member this.Stream = stream
        member this.Client = client
        member this.Address = server + ":" + string port

        member this.Close =
            (this :> IDisposable).Dispose()

        interface IDisposable with
            member this.Dispose() =
                printfn "Disposing TCP Client"
                stream.Close()
                client.Close()

    /// Attempts to connect with the given server on the given port through TCP
    let startClient (server: string) (port: int) = 
        try
            let client = new TCPClient(server, port)
            Some client
        with
        | :? ArgumentNullException as ane -> (*printfn "ArgumentNullException %s" ane.Message;*) None
        | :? SocketException as se -> (*printfn "SocketException %s" se.Message;*) None
