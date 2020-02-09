#load "../Utils/TcpClientHelpers.fsx"

namespace FuncIRC

open System
open System.IO
open System.Net.Sockets
open System.Net.Security
open System.Security.Authentication
open System.Security.Cryptography.X509Certificates

open TcpClientHelpers

#if !DEBUG
module internal TCPClient =
#else
module TCPClient =
#endif

    /// Wrapper for TcpClient and SslStream
    [<Sealed>]
    type TCPClient(server: string, port: int, useSsl: bool) =
        let client: TcpClient ref = ref null
        let stream: Stream ref = ref null

        /// Writes byte array data to stream
        member this.WriteToStream messageData = 
            stream.Value.Write (messageData, 0, messageData.Length)

        /// Reads byte array data from stream
        member this.ReadFromStream (data: byte array) (startOffset: int) (length: int) =
            stream.Value.Read (data, startOffset, data.Length)

        member this.Client    = client.Value
        member this.Connected = this.Client.Connected
        member this.Address   = server + ":" + string port

        /// Connects client to server if it's not already connected
        /// TODO: Make this Async
        member this.Connect: bool =
            if not (isNull client.Value) && this.Connected then true
            else // F# requires every branch of if statements to return the same value as the first branch
            if not (isNull client.Value) then this.Close // Close TcpClient since it wasn't connected

            try
                client := new TcpClient (server, port)

                if useSsl then
                    new SslStream (client.Value.GetStream(), false, noSslErrors)
                    |> fun s ->
                        s.AuthenticateAsClient(server, X509CertificateCollection(), SslProtocols.None, true)
                        stream := (s :> Stream)
                else
                    stream := client.Value.GetStream() :> Stream

                true
            with
            | :? ArgumentNullException as ane -> printfn "ArgumentNullException %s" ane.Message; false
            | :? SocketException as se -> printfn "SocketException %s" se.Message; false
            | :? System.IO.IOException as ioe -> printfn "IOException: %s" ioe.Message; false

        /// Closes and disposes the TcpClient and IO Stream
        member this.Close = (this :> IDisposable).Dispose()

        interface IDisposable with
            member this.Dispose() =
                printfn "Disposing TCP Client"
                if not (isNull stream.Value) then
                    stream.Value.Close()
                    stream.Value.Dispose()
                    stream := null

                if not (isNull client.Value) then
                    client.Value.Close()
                    client.Value.Dispose()
                    client := null
