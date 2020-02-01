namespace FuncIRC

open System
open System.Net.Sockets
open System.Net.Security

module internal ConnectionClient =
    exception ClientConnectionException

    /// Wrapper for TcpClient
    [<Sealed>]
    type TCPClient(server: string, port: int, ?useSsl: bool) =
        let client: TcpClient ref = ref null
        let networkStream: NetworkStream ref = ref null
        let sslStream: SslStream ref = ref null

        let (|UsingSSL|NoSSL|) stream =
            match useSsl with
            | Some useSsl when useSsl -> UsingSSL sslStream
            | _ -> NoSSL networkStream

        member this.WriteToStream messageData = 
            match useSsl with
            | NoSSL stream -> stream.Value.Write (messageData, 0, messageData.Length)
            | UsingSSL sslStream -> sslStream.Value.Write (messageData, 0, messageData.Length)

        member this.ReadFromStream (data: byte array) (startOffset: int) (length: int) =
            match useSsl with
            | NoSSL stream -> stream.Value.Read (data, 0, data.Length)
            | UsingSSL sslStream -> sslStream.Value.Read (data, 0, data.Length)

        member this.Client = client.Value
        member this.Connected = this.Client.Connected
        member this.Address = server + ":" + string port

        member this.Connect: bool =
            if not (isNull client.Value) && this.Connected then true
            else 
            if not (isNull client.Value) then this.Close

            try
                client := new TcpClient (server, port)
                networkStream := client.Value.GetStream()
                true
            with
            | :? ArgumentNullException as ane -> (*printfn "ArgumentNullException %s" ane.Message;*) false
            | :? SocketException as se -> (*printfn "SocketException %s" se.Message;*) false

        member this.Close =
            (this :> IDisposable).Dispose()

        interface IDisposable with
            member this.Dispose() =
                printfn "Disposing TCP Client"
                networkStream.Value.Close()
                sslStream.Value.Close()
                client.Value.Close()

                networkStream.Value.Dispose()
                sslStream.Value.Dispose()
                client.Value.Dispose()

                networkStream := null
                sslStream := null
                client := null

    /// Attempts to connect with the given server on the given port through TCP
    /// Returns None if .NET TcpClient threw an exception
    let startClient (server: string) (port: int) =
        new TCPClient(server, port)
