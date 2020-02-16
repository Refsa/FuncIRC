namespace FuncIRC

open System.Net

module internal ExternalIPAddress =

    /// <summary> polls an external server to get the external IP of this device </summary>
    let private getExternalIPAddressRemote() = 
        try
            use webc = new WebClient()
            webc.DownloadString("http://bot.whatismyipaddress.com")
        with
        | _ -> ""

    let private isPrivateIP (ip: IPAddress) =
        if ip.AddressFamily = Sockets.AddressFamily.InterNetwork then
            let ipBytes = ip.GetAddressBytes()

            match ipBytes with
            | _ when (int) ipBytes.[0] = 10 -> true
            | _ when (int) ipBytes.[0] = 172 && (int) ipBytes.[1] = 16 -> true
            | _ when (int) ipBytes.[0] = 192 && (int) ipBytes.[1] = 168 -> true
            | _ when (int) ipBytes.[0] = 169 && (int) ipBytes.[1] = 254 -> true
            | _ -> false
        else
            false

    let private getExternalIPAddressLocal() =
        let hostEntries = Dns.GetHostEntry (Dns.GetHostName())
        hostEntries.AddressList
        |> Array.tryFind (
            fun e ->
                if e.AddressFamily = Sockets.AddressFamily.InterNetwork then
                    not (isPrivateIP e)
                else false
        )

    let getExternalIPAddress(tryExternal: bool) = 
        try 
            let eip = getExternalIPAddressLocal()
            eip.ToString()
        with
        | _ -> 
            if tryExternal then getExternalIPAddressRemote()
            else ""