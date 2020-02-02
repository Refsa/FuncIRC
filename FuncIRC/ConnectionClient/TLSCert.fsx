(*
namespace FuncIRC
open System.Security.Cryptography.X509Certificates
open System.Net
open System
open System.Security.Cryptography

module TLSCert =

    let buildSelfSignedCertificate() =
        let certificateName = "FuncIRC_TLS_" + Environment.MachineName
        let sanBuilder = SubjectAlternativeNameBuilder()
        sanBuilder.AddIpAddress IPAddress.Loopback
        sanBuilder.AddIpAddress IPAddress.IPv6Loopback
        sanBuilder.AddDnsName "localhost"
        sanBuilder.AddUserPrincipalName Environment.MachineName

        let distinguishedName = X500DistinguishedName (String.Format("CN={0}", certificateName))

        use rsa = RSA.Create("2048")
        let request = CertificateRequest (distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)

        request.CertificateExtensions.Add (X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment ||| 
                                                                 X509KeyUsageFlags.KeyEncipherment ||| 
                                                                 X509KeyUsageFlags.DigitalSignature, 
                                                                 false))

        let oid = Oid ("1.3.6.1.5.5.7.3.1")
        let oidCollection = OidCollection ()
        oidCollection.Add oid |> ignore

        request.CertificateExtensions.Add (X509EnhancedKeyUsageExtension (oidCollection, false))
        request.CertificateExtensions.Add (sanBuilder.Build())

        let certificate = request.CreateSelfSigned (DateTimeOffset(DateTime.UtcNow.AddDays(-1.0)), DateTimeOffset(DateTime.UtcNow.AddDays(3650.0)))
        certificate.FriendlyName <- certificateName

        new X509Certificate2 (certificate.Export (X509ContentType.Pfx, "password"), "password", X509KeyStorageFlags.MachineKeySet)
*)