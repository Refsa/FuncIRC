#load "ConnectionClient.fsx"
#load "IRCClient.fsx"

namespace FuncIRC

module IRCStreamWriter =
    open System.Text
    open IRCClient
    open ConnectionClient

    let sendIrcMessage (client: Client) (message: string) =
        let messageData =
            match message with
            | message when message.EndsWith("\r\n") -> Encoding.UTF8.GetBytes (message)
            | _ -> Encoding.UTF8.GetBytes (message + "\r\n")

        client.Stream.Write (messageData, 0, messageData.Length)

    let sendRegistrationMessage (client: Client) (nick: string, user: string, realName: string, pass: string) =
        let message = 
            match () with
            | _ when pass <> "" && nick <> "" && user <> "" -> "CAP LS 302\r\nPASS " + pass + "\r\nNICK " + nick + "\r\nUSER 0 * " + user + "\r\n"
            | _ when nick <> "" && user <> "" -> "CAP LS 302\r\nNICK " + nick + "\r\nUSER " + user + " 0 * " + realName + "\r\n"
            | _ -> raise RegistrationContentException

        sendIrcMessage client message