namespace FuncIRC

open System.Net.Security
open System.Security.Authentication
open System.Security.Cryptography.X509Certificates

module TcpClientHelpers =

    /// Callback wrapper for RemoteCertificateValidationCallback
    let inline validateCertCallback cb = new RemoteCertificateValidationCallback(cb)

#if DEBUG
    // IMPORTANT: This is not a safe way to handle self-signed/unnamed certs but should not be a problem against production servers
    //            Only to be used in Debug build
    let noSslErrors = validateCertCallback (fun _ _ _ errors -> printfn "SSL Certificate Errors: %s" (errors.ToString()); true)
#else
    let noSslErrors = validateCertCallback (fun _ _ _ errors -> errors = SslPolicyErrors.None)
#endif