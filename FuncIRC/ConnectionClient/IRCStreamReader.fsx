#load "ConnectionClient.fsx"

namespace FuncIRC
open System.Text
open System.Threading

module IRCStreamReader =
    open ConnectionClient

    let readStream (client: Client) (callback: string * Client -> unit) =
        let rec readLoop() =
            async {
                try
                    let data =
                        [| for i in 0 .. 512 -> byte 0 |]

                    let bytes = client.Stream.Read(data, 0, data.Length)

                    let receivedData = Encoding.UTF8.GetString(data, 0, bytes)

                    callback (receivedData, client)

                    return! readLoop()
                with e -> printfn "Exception: %s" e.Message
            }

        readLoop()

    let receivedDataHandler (data: string, client: Client) =
        match data with
        | "" -> ()
        | _ -> printfn "%s" data
