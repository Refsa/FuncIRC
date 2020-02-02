#load "TLSCert.fsx"

namespace FuncIRC

open TLSCert
open System
open System.Net.Sockets
open System.Net.Security
open System.Security.Authentication
open System.Security.Cryptography.X509Certificates

module internal ConnectionClient =
    exception ClientConnectionException

    let inline validateCertCallback cb = new RemoteCertificateValidationCallback(cb)
    let noSslErrors = validateCertCallback (fun _ _ _ errors -> true)

    /// Wrapper for TcpClient
    [<Sealed>]
    type TCPClient(server: string, port: int, useSsl: bool) =
        let client: TcpClient ref = ref null
        let networkStream: NetworkStream ref = ref null
        let sslStream: SslStream ref = ref null

        /// Writes byte array data to stream
        member this.WriteToStream messageData = 
            match useSsl with
            | false -> networkStream.Value.Write (messageData, 0, messageData.Length)
            | true -> sslStream.Value.Write (messageData, 0, messageData.Length)

        /// Reads byte array data from stream
        member this.ReadFromStream (data: byte array) (startOffset: int) (length: int) =
            match useSsl with
            | false -> networkStream.Value.Read (data, 0, data.Length)
            | true -> sslStream.Value.Read (data, 0, data.Length)

        member this.Client = client.Value
        member this.Connected = this.Client.Connected
        member this.Address = server + ":" + string port

        /// Connects client to server if it's not already connected
        member this.Connect: bool =
            if not (isNull client.Value) && this.Connected then true
            else 
            if not (isNull client.Value) then this.Close

            try
                client := new TcpClient (server, port)
                networkStream := client.Value.GetStream()

                if useSsl then
                    sslStream := new SslStream (client.Value.GetStream(), false, noSslErrors)
                    sslStream.Value.AuthenticateAsClient(server, X509CertificateCollection(), SslProtocols.None, true)

                true
            with
            | :? ArgumentNullException as ane -> (*printfn "ArgumentNullException %s" ane.Message;*) false
            | :? SocketException as se -> (*printfn "SocketException %s" se.Message;*) false
            | :? System.IO.IOException as ioe -> printfn "IOException: %s" ioe.Message; false

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
    let startClient (server: string) (port: int) (useSsl: bool) =
        new TCPClient(server, port, useSsl)
