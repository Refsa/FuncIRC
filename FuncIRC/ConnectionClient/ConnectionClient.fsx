namespace FuncIRC

open System
open System.Net.Sockets

module internal ConnectionClient =
    exception ClientConnectionException

    /// Wrapper for TcpClient
    [<Sealed>]
    type TCPClient(server: string, port: int) =
        let client: TcpClient ref = ref null
        let stream: NetworkStream ref = ref null

        member this.Stream = stream.Value
        member this.Client = client.Value
        member this.Connected = this.Client.Connected
        member this.Address = server + ":" + string port

        member this.Connect: bool =
            if not (isNull client.Value) && this.Connected then true
            else 
            if not (isNull client.Value) then this.Close

            try
                client := new TcpClient (server, port)
                stream := client.Value.GetStream()
                true
            with
            | :? ArgumentNullException as ane -> (*printfn "ArgumentNullException %s" ane.Message;*) false
            | :? SocketException as se -> (*printfn "SocketException %s" se.Message;*) false

        member this.Close =
            (this :> IDisposable).Dispose()

        interface IDisposable with
            member this.Dispose() =
                printfn "Disposing TCP Client"
                stream.Value.Close()
                client.Value.Close()
                stream.Value.Dispose()
                client.Value.Dispose()
                stream := null
                client := null

    /// Attempts to connect with the given server on the given port through TCP
    /// Returns None if .NET TcpClient threw an exception
    let startClient (server: string) (port: int) =
        new TCPClient(server, port)
