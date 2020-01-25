#load "ConnectionClient.fsx"
#load "IRCClient.fsx"

namespace FuncIRC

module IRCStreamWriter =
    open System.Text
    open IRCClient
    open ConnectionClient

    /// Verifies and sends the message string to the client stream
    /// Preferred syntax for sending messages is client |> sendIrcMessage <| message
    let sendIrcMessage (client: Client) (message: string) =
        let messageData =
            match message with
            | message when message.EndsWith("\r\n") -> Encoding.UTF8.GetBytes (message)
            | _ -> Encoding.UTF8.GetBytes (message + "\r\n")

        client.Stream.Write (messageData, 0, messageData.Length)

    /// Creates a registration message and sends it to the client
    let sendRegistrationMessage (client: Client) (nick: string, user: string, realName: string, pass: string) =
        let message = 
            match () with
            | _ when pass <> "" && nick <> "" && user <> "" -> "CAP LS 302\r\nPASS " + pass + "\r\nNICK " + nick + "\r\nUSER 0 * " + user + "\r\n"
            | _ when nick <> "" && user <> "" -> "CAP LS 302\r\nNICK " + nick + "\r\nUSER " + user + " 0 * " + realName + "\r\n"
            | _ -> raise RegistrationContentException

        client |> sendIrcMessage <| message