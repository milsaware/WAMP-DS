using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace WAMP_DS.Managers
{
    public class CertificateManager
    {
        private readonly string apacheDirectory;
        private readonly string opensslExecutable;
        private readonly string opensslConfig;

        private readonly string certificateDirectory;
        private readonly string rootCaCertificate;
        private readonly string rootCaPrivateKey;
        private readonly string serverCertificate;
        private readonly string serverPrivateKey;

        public string RootCaCertificate =>
            rootCaCertificate;

        public string ServerCertificate =>
            serverCertificate;

        public string ServerPrivateKey =>
            serverPrivateKey;

        public CertificateManager()
        {
            apacheDirectory =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "runtimes",
                    "apache",
                    "2.4.68"
                );

            opensslExecutable =
                Path.Combine(
                    apacheDirectory,
                    "bin",
                    "openssl.exe"
                );

            opensslConfig =
                Path.Combine(
                    apacheDirectory,
                    "conf",
                    "openssl.cnf"
                );

            certificateDirectory =
                Path.Combine(
                    apacheDirectory,
                    "conf",
                    "certs"
                );

            rootCaCertificate =
                Path.Combine(
                    certificateDirectory,
                    "wamp-ds-root-ca.crt"
                );

            rootCaPrivateKey =
                Path.Combine(
                    certificateDirectory,
                    "wamp-ds-root-ca.key"
                );

            serverCertificate =
                Path.Combine(
                    apacheDirectory,
                    "conf",
                    "server.crt"
                );

            serverPrivateKey =
                Path.Combine(
                    apacheDirectory,
                    "conf",
                    "server.key"
                );
        }

        public void EnsureHttpsCertificate(
    IEnumerable<string> domains)
        {
            ValidateRequirements();

            Directory.CreateDirectory(
                certificateDirectory
            );

            EnsureRootCertificateAuthority();

            EnsureServerCertificate(
                domains
            );

            EnsureRootCertificateTrusted();
        }

        public X509Certificate2? GetServerCertificate()
        {
            if (!File.Exists(serverCertificate))
                return null;

            try
            {
                return new X509Certificate2(
                    serverCertificate
                );
            }
            catch
            {
                return null;
            }
        }

        public X509Certificate2? GetRootCertificate()
        {
            if (!File.Exists(rootCaCertificate))
                return null;

            try
            {
                return new X509Certificate2(
                    rootCaCertificate
                );
            }
            catch
            {
                return null;
            }
        }

        public bool IsServerCertificateTrusted()
        {
            X509Certificate2? certificate =
                GetServerCertificate();

            X509Certificate2? rootCertificate =
                GetRootCertificate();

            if (certificate == null ||
                rootCertificate == null)
            {
                return false;
            }

            try
            {
                using X509Chain chain =
                    new X509Chain();

                chain.ChainPolicy =
                    new X509ChainPolicy
                    {
                        RevocationMode =
                            X509RevocationMode.NoCheck,

                        VerificationFlags =
                            X509VerificationFlags.AllowUnknownCertificateAuthority,

                        TrustMode =
                            X509ChainTrustMode.CustomRootTrust
                    };

                chain.ChainPolicy
                    .CustomTrustStore
                    .Add(
                        rootCertificate
                    );

                return chain.Build(
                    certificate
                );
            }
            catch
            {
                return false;
            }
        }

        private void ValidateRequirements()
        {
            if (!File.Exists(opensslExecutable))
            {
                throw new FileNotFoundException(
                    "OpenSSL was not found in the Apache runtime.",
                    opensslExecutable
                );
            }

            if (!File.Exists(opensslConfig))
            {
                throw new FileNotFoundException(
                    "OpenSSL configuration file was not found.",
                    opensslConfig
                );
            }
        }

        private void EnsureRootCertificateAuthority()
        {
            if (File.Exists(rootCaCertificate) &&
                File.Exists(rootCaPrivateKey))
            {
                return;
            }

            DeleteCertificateFiles(
                rootCaCertificate,
                rootCaPrivateKey
            );

            RunOpenSsl(
                $"req -x509 -nodes -days 3650 " +
                $"-newkey rsa:4096 " +
                $"-keyout \"{rootCaPrivateKey}\" " +
                $"-out \"{rootCaCertificate}\" " +
                $"-config \"{opensslConfig}\" " +
                $"-subj \"/C=GB/ST=Local/L=Local/O=WAMP-DS/OU=Development/CN=WAMP-DS Local Development CA\" " +
                $"-addext \"basicConstraints=critical,CA:TRUE,pathlen:1\" " +
                $"-addext \"keyUsage=critical,keyCertSign,cRLSign\" " +
                $"-addext \"subjectKeyIdentifier=hash\""
            );

            if (!File.Exists(rootCaCertificate) ||
                !File.Exists(rootCaPrivateKey))
            {
                throw new InvalidOperationException(
                    "OpenSSL completed but the WAMP-DS Root CA files were not created."
                );
            }
        }

        private void EnsureRootCertificateTrusted()
        {
            string certutilExecutable =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.System
                    ),
                    "certutil.exe"
                );

            if (!File.Exists(certutilExecutable))
            {
                throw new FileNotFoundException(
                    "Windows Certificate Utility (certutil.exe) was not found.",
                    certutilExecutable
                );
            }

            ProcessStartInfo startInfo =
                new ProcessStartInfo
                {
                    FileName = certutilExecutable,
                    Arguments =
                        $"-addstore -user Root \"{rootCaCertificate}\"",
                    WorkingDirectory =
                        certificateDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

            using Process process =
                new Process
                {
                    StartInfo = startInfo
                };

            process.Start();

            string output =
                process.StandardOutput.ReadToEnd();

            string error =
                process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string message =
                    string.IsNullOrWhiteSpace(error)
                        ? output
                        : error;

                throw new InvalidOperationException(
                    $"Windows was unable to trust the WAMP-DS local development certificate authority.\n\n{message}"
                );
            }

            VerifyRootCertificateTrusted();
        }

        private void VerifyRootCertificateTrusted()
        {
            X509Certificate2 rootCertificate =
                new X509Certificate2(
                    rootCaCertificate
                );

            using X509Store certificateStore =
                new X509Store(
                    StoreName.Root,
                    StoreLocation.CurrentUser
                );

            certificateStore.Open(
                OpenFlags.ReadOnly
            );

            X509Certificate2Collection matchingCertificates =
                certificateStore.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    rootCertificate.Thumbprint,
                    false
                );

            certificateStore.Close();

            if (matchingCertificates.Count == 0)
            {
                throw new InvalidOperationException(
                    "The WAMP-DS local development certificate authority was added, but Windows could not verify that it exists in the Trusted Root Certification Authorities store."
                );
            }
        }

        private void EnsureServerCertificate(
            IEnumerable<string> domains)
        {
            if (IsServerCertificateValid())
            {
                return;
            }

            DeleteCertificateFiles(
                serverCertificate,
                serverPrivateKey
            );

            string certificateRequest =
                Path.Combine(
                    certificateDirectory,
                    "server.csr"
                );

            string certificateExtensions =
                Path.Combine(
                    certificateDirectory,
                    "server.ext"
                );

            string serialFile =
                Path.Combine(
                    certificateDirectory,
                    "wamp-ds-ca.srl"
                );

            try
            {
                CreateServerCertificateRequest(
                    certificateRequest
                );

                CreateCertificateExtensions(
                    certificateExtensions,
                    domains
                );

                RunOpenSsl(
                    $"x509 -req " +
                    $"-in \"{certificateRequest}\" " +
                    $"-CA \"{rootCaCertificate}\" " +
                    $"-CAkey \"{rootCaPrivateKey}\" " +
                    $"-CAcreateserial " +
                    $"-out \"{serverCertificate}\" " +
                    $"-days 825 " +
                    $"-sha256 " +
                    $"-extfile \"{certificateExtensions}\""
                );
            }
            finally
            {
                DeleteCertificateFiles(
                    certificateRequest,
                    certificateExtensions,
                    serialFile
                );
            }

            if (!File.Exists(serverCertificate) ||
                !File.Exists(serverPrivateKey))
            {
                throw new InvalidOperationException(
                    "OpenSSL completed but the HTTPS server certificate files were not created."
                );
            }
        }

        private bool IsServerCertificateValid()
        {
            if (!File.Exists(serverCertificate) ||
                !File.Exists(serverPrivateKey) ||
                !File.Exists(rootCaCertificate))
            {
                return false;
            }

            try
            {
                X509Certificate2 certificate =
                    new X509Certificate2(
                        serverCertificate
                    );

                X509Certificate2 rootCertificate =
                    new X509Certificate2(
                        rootCaCertificate
                    );

                DateTime utcNow =
                    DateTime.UtcNow;

                if (utcNow < certificate.NotBefore.ToUniversalTime() ||
                    utcNow > certificate.NotAfter.ToUniversalTime())
                {
                    return false;
                }

                X509Extension? subjectAlternativeName =
                    certificate.Extensions
                        .Cast<X509Extension>()
                        .FirstOrDefault(
                            extension =>
                                extension.Oid?.Value == "2.5.29.17"
                        );

                if (subjectAlternativeName == null)
                {
                    return false;
                }

                string sanText =
                    subjectAlternativeName.Format(
                        false
                    );

                if (!sanText.Contains(
                    "localhost",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (!sanText.Contains(
                    "127.0.0.1",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (!sanText.Contains(
                    "::1",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                using X509Chain chain =
                    new X509Chain();

                chain.ChainPolicy =
                    new X509ChainPolicy
                    {
                        RevocationMode =
                            X509RevocationMode.NoCheck,

                        VerificationFlags =
                            X509VerificationFlags.AllowUnknownCertificateAuthority
                    };

                chain.ChainPolicy
                    .TrustMode =
                    X509ChainTrustMode.CustomRootTrust;

                chain.ChainPolicy
                    .CustomTrustStore
                    .Add(
                        rootCertificate
                    );

                bool chainValid =
                    chain.Build(
                        certificate
                    );

                if (!chainValid)
                {
                    return false;
                }

                if (chain.ChainElements.Count < 2)
                {
                    return false;
                }

                X509Certificate2 chainRoot =
                    chain.ChainElements[
                        chain.ChainElements.Count - 1
                    ].Certificate;

                if (!string.Equals(
                    chainRoot.Thumbprint,
                    rootCertificate.Thumbprint,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void CreateServerCertificateRequest(
            string certificateRequest)
        {
            RunOpenSsl(
                $"req -new -nodes " +
                $"-newkey rsa:2048 " +
                $"-keyout \"{serverPrivateKey}\" " +
                $"-out \"{certificateRequest}\" " +
                $"-config \"{opensslConfig}\" " +
                $"-subj \"/C=GB/ST=Local/L=Local/O=WAMP-DS/OU=Development/CN=localhost\""
            );
        }

        private void CreateCertificateExtensions(
    string certificateExtensions,
    IEnumerable<string> domains)
        {
            List<string> lines = new()
    {
        "authorityKeyIdentifier=keyid,issuer",
        "basicConstraints=critical,CA:FALSE",
        "keyUsage=critical,digitalSignature,keyEncipherment",
        "extendedKeyUsage=serverAuth",
        "subjectAltName=@alt_names",
        "",
        "[alt_names]"
    };


            int dnsIndex = 1;
            int ipIndex = 1;


            foreach (string domain in domains.Distinct())
            {
                if (System.Net.IPAddress.TryParse(domain, out _))
                {
                    lines.Add(
                        $"IP.{ipIndex}={domain}"
                    );

                    ipIndex++;
                }
                else
                {
                    lines.Add(
                        $"DNS.{dnsIndex}={domain}"
                    );

                    dnsIndex++;
                }
            }


            File.WriteAllText(
                certificateExtensions,
                string.Join(
                    Environment.NewLine,
                    lines
                )
            );
        }

        public void RegenerateServerCertificate(
    IEnumerable<string> domains)
        {
            DeleteCertificateFiles(
                serverCertificate,
                serverPrivateKey
            );

            EnsureServerCertificate(
                domains
            );
        }

        private void RunOpenSsl(
            string arguments)
        {
            ProcessStartInfo startInfo =
                new ProcessStartInfo
                {
                    FileName = opensslExecutable,
                    Arguments = arguments,
                    WorkingDirectory = apacheDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

            using Process process =
                new Process
                {
                    StartInfo = startInfo
                };

            process.Start();

            string output =
                process.StandardOutput.ReadToEnd();

            string error =
                process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string message =
                    string.IsNullOrWhiteSpace(error)
                        ? output
                        : error;

                throw new InvalidOperationException(
                    $"OpenSSL failed to create the WAMP-DS HTTPS certificate.\n\n{message}"
                );
            }
        }

        private void DeleteCertificateFiles(
            params string[] files)
        {
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    File.Delete(
                        file
                    );
                }
            }
        }

        public (string Certificate, string Key) CreateCertificateForHost(
    string domain)
        {
            string hostDirectory =
                Path.Combine(
                    certificateDirectory,
                    domain
                );

            Directory.CreateDirectory(
                hostDirectory
            );


            string certificate =
                Path.Combine(
                    hostDirectory,
                    "server.crt"
                );


            string key =
                Path.Combine(
                    hostDirectory,
                    "server.key"
                );


            // generate certificate here

            return (
                certificate,
                key
            );
        }
    }
}