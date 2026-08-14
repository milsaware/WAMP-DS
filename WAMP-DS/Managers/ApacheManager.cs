using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using WAMP_DS.Core;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public enum ApacheStatus
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Failed
    }

    public class ApacheManager
    {
        private Process? apacheProcess;

        private readonly HostsManager hostsManager;
        private readonly string apacheDirectory;
        private readonly string apacheExecutable;
        private readonly string apacheConfig;
        private readonly string apacheVirtualHostsConfig;
        private readonly string apacheSslConfig;
        private readonly InstallationPaths installationPaths;
        private readonly CertificateManager certificateManager;
        private readonly ApacheConfigGenerator configGenerator;

        public string VirtualHostsConfig => apacheVirtualHostsConfig;

        public string ApacheDirectory => apacheDirectory;

        private const int apachePort = 80;
        private const int httpsPort = 443;

        public ApacheStatus Status { get; private set; } =
            ApacheStatus.Stopped;

        public string ServerCertificate =>
            certificateManager.ServerCertificate;

        public string ServerPrivateKey =>
            certificateManager.ServerPrivateKey;

        public int Port =>
            apachePort;

        public int HttpsPort =>
            httpsPort;

        public string Version =>
            "2.4.68";

        public string ConfigurationFile =>
            apacheConfig;

        public event EventHandler? StatusChanged;

        public bool IsRunning =>
            apacheProcess != null &&
            !apacheProcess.HasExited;

        public ApacheManager(
    InstallationPaths installationPaths)
        {
            this.installationPaths = installationPaths;
            apacheDirectory =
    installationPaths.ApachePath;

            apacheExecutable = Path.Combine(
                apacheDirectory,
                "bin",
                "httpd.exe"
            );

            apacheVirtualHostsConfig =
                Path.Combine(
                    apacheDirectory,
                    "conf",
                    "extra",
                    "httpd-ssl.conf"
                );

            apacheSslConfig =
                Path.Combine(
                    apacheDirectory,
                    "conf",
                    "extra",
                    "httpd-ssl.conf"
                );

            apacheConfig = Path.Combine(
                apacheDirectory,
                "conf",
                "httpd.conf"
            );

            certificateManager =
    new CertificateManager();

            hostsManager =
    new HostsManager();


            configGenerator =
                new ApacheConfigGenerator(
                    installationPaths
                );
        }

        public string ReadVhostsConfiguration()
        {
            return File.ReadAllText(
                apacheVirtualHostsConfig
            );
        }


        public void SaveVhostsConfiguration(
            string content)
        {
            File.WriteAllText(
                apacheVirtualHostsConfig,
                content
            );
        }

        public string VirtualHostsConfigurationFile =>
            apacheVirtualHostsConfig;

        public List<ApacheModule> GetAvailableModules()
        {
            List<ApacheModule> modules = new();

            if (!File.Exists(apacheConfig))
                return modules;


            foreach (string line in File.ReadAllLines(apacheConfig))
            {
                string trimmed = line.Trim();


                if (!trimmed.Contains("LoadModule"))
                    continue;


                bool enabled =
                    !trimmed.StartsWith("#");


                string directive =
                    trimmed.TrimStart('#')
                           .Trim();


                string[] parts =
                    directive.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries
                    );


                if (parts.Length < 3)
                    continue;


                string moduleName =
                    parts[1];


                ApacheModule module = new()
                {
                    Name = moduleName.Replace(
        "_module",
        ""
    ),

                    Directive = moduleName,

                    IsEnabled = enabled
                };

                switch (module.Name)
                {
                    case "auth_form":
                        module.Dependencies.Add(
                            "session_module"
                        );

                        module.Dependencies.Add(
                            "session_cookie_module"
                        );

                        module.Dependencies.Add(
                            "request_module"
                        );

                        break;
                }


                modules.Add(module);
            }


            return modules;
        }

        public List<ApacheVirtualHost> ReadVirtualHosts()
        {
            List<ApacheVirtualHost> hosts = new();


            if (!File.Exists(apacheVirtualHostsConfig))
                return hosts;


            string content =
                File.ReadAllText(
                    apacheVirtualHostsConfig
                );


            MatchCollection matches =
                Regex.Matches(
                    content,
                    @"<VirtualHost\s+\*:(80|443)>(.*?)</VirtualHost>",
                    RegexOptions.Singleline
                );


            foreach (Match match in matches)
            {
                string port =
                    match.Groups[1].Value;


                string block =
                    match.Groups[2].Value;


                ApacheVirtualHost host =
                    new ApacheVirtualHost
                    {
                        HttpsEnabled = port == "443"
                    };

                Match serverName =
                    Regex.Match(
                        block,
                        @"ServerName\s+(.+)"
                    );


                if (serverName.Success)
                {
                    host.ServerName =
                        serverName.Groups[1].Value.Trim();
                }


                Match documentRoot =
                    Regex.Match(
                        block,
                        @"DocumentRoot\s+""(.+)"""
                    );


                if (documentRoot.Success)
                {
                    host.DocumentRoot =
                        documentRoot.Groups[1].Value;
                }


                Match directory =
                    Regex.Match(
                        block,
                        @"<Directory\s+""([^""]+)""\s*>",
                        RegexOptions.IgnoreCase
                    );


                if (directory.Success)
                {
                    host.Directory =
                        directory.Groups[1].Value;
                }

                Match directoryBlock =
    Regex.Match(
        block,
        @"<Directory\s+""(.+)"">(.*?)</Directory>",
        RegexOptions.Singleline
    );


                if (directoryBlock.Success)
                {
                    string rules =
                        directoryBlock.Groups[2].Value;

                    Match allowOverride =
                        Regex.Match(
                            rules,
                            @"AllowOverride\s+(.+)"
                        );

                    if (allowOverride.Success)
                    {
                        host.AllowOverride =
                            allowOverride.Groups[1].Value.Trim();
                    }

                    Match require =
                        Regex.Match(
                            rules,
                            @"Require\s+(.+)"
                        );

                    if (require.Success)
                    {
                        host.RequireValue =
                            require.Groups[1]
                                   .Value
                                   .Trim();
                    }

                    host.OptionsIndexes =
                        Regex.IsMatch(
                            rules,
                            @"Options.*Indexes"
                        );

                    host.OptionsFollowSymLinks =
                        Regex.IsMatch(
                            rules,
                            @"Options.*FollowSymLinks"
                        );

                    host.RewriteEngine =
                        Regex.IsMatch(
                            rules,
                            @"RewriteEngine\s+On",
                            RegexOptions.IgnoreCase
                        );
                }

                Match errorLog =
                    Regex.Match(
                        block,
                        @"ErrorLog\s+""(.+)"""
                    );

                if (errorLog.Success)
                {
                    host.ErrorLog =
                        errorLog.Groups[1].Value;
                }

                Match customLog =
                    Regex.Match(
                        block,
                        @"CustomLog\s+""(.+)"""
                    );

                if (customLog.Success)
                {
                    host.CustomLog =
                        customLog.Groups[1].Value;
                }

                hosts.Add(host);
            }

            return hosts;
        }

        private string ExtractValue(
    string block,
    string key)
        {
            foreach (string line in block.Split('\n'))
            {
                string trimmed =
                    line.Trim();

                if (trimmed.StartsWith(key))
                {
                    return trimmed
                        .Substring(key.Length)
                        .Trim();
                }
            }

            return "";
        }

        private string ExtractDirectory(
    string block)
        {
            int start =
                block.IndexOf("<Directory");

            int end =
                block.IndexOf(
                    "</Directory>"
                );

            if (start == -1 || end == -1)
                return "";

            return block
                .Substring(
                    start,
                    end - start
                );
        }

        public async Task SaveVirtualHosts(
    List<ApacheVirtualHost> hosts)
        {
            foreach (ApacheVirtualHost host in hosts)
            {
                if (host.HttpsEnabled)
                {
                    PrepareVirtualHostCertificate(host);
                }
            }

            hostsManager.AddHosts(
                hosts.Select(x => x.ServerName)
            );

            SaveHttpVirtualHosts(
                hosts.Where(x => !x.HttpsEnabled)
            );

            SaveHttpsVirtualHosts(
                hosts.Where(x => x.HttpsEnabled)
            );

            await RegenerateHttpsCertificate(hosts);
        }

        private void SaveHttpVirtualHosts(
    IEnumerable<ApacheVirtualHost> hosts)
        {
            using StreamWriter writer =
                new(apacheVirtualHostsConfig);

            writer.WriteLine(
        @"#
# WAMP-DS HTTP Virtual Hosts
#
# Managed by WAMP-DS
#
");


            foreach (ApacheVirtualHost host in hosts)
            {
                writer.WriteLine(
        $@"
<VirtualHost *:80>

    ServerName {host.ServerName}

    DocumentRoot ""{host.DocumentRoot}""

    <Directory ""{host.Directory}"">
        AllowOverride {host.AllowOverride}
        Require all granted
    </Directory>

    ErrorLog ""{host.ErrorLog}""

    CustomLog ""{host.CustomLog}"" common

</VirtualHost>
");
            }
        }

        private void SaveHttpsVirtualHosts(
    IEnumerable<ApacheVirtualHost> hosts)
        {
            using StreamWriter writer =
                new(apacheSslConfig);


            writer.WriteLine(
        @"#
# WAMP-DS HTTPS Virtual Hosts
#
# Managed by WAMP-DS
#
");


            foreach (ApacheVirtualHost host in hosts)
            {
                writer.WriteLine(
$@"
<VirtualHost *:443>
    ServerName {host.ServerName}
    DocumentRoot ""{host.DocumentRoot}""
    <Directory ""{host.Directory}"">
        AllowOverride {host.AllowOverride}
        Require all granted
    </Directory>
    {host.CustomConfiguration}
    SSLEngine on
    SSLCertificateFile ""${{SRVROOT}}/conf/server.crt""
    SSLCertificateKeyFile ""${{SRVROOT}}/conf/server.key""

    ErrorLog ""{host.ErrorLog}""
    CustomLog ""{host.CustomLog}"" common
</VirtualHost>
");
            }
        }

        private async Task RegenerateHttpsCertificate(
    IEnumerable<ApacheVirtualHost> hosts)
        {
            List<string> httpsDomains =
                hosts
                    .Where(x => x.HttpsEnabled)
                    .Select(x => x.ServerName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();


            if (httpsDomains.Count == 0)
            {
                return;
            }


            httpsDomains.Add("localhost");
            httpsDomains.Add("127.0.0.1");
            httpsDomains.Add("::1");


            certificateManager
                .RegenerateServerCertificate(
                    httpsDomains
                );


            if (IsRunning)
            {
                Stop();

                await Task.Delay(3000);

                await StartAsync();
            }
        }

        private void GracefulRestart()
        {
            if (!IsRunning)
                return;


            ProcessStartInfo startInfo =
                new ProcessStartInfo
                {
                    FileName = apacheExecutable,

                    Arguments =
                        $"-k graceful -f \"{apacheConfig}\" -d \"{apacheDirectory}\"",

                    WorkingDirectory =
                        apacheDirectory,

                    UseShellExecute = false,

                    CreateNoWindow = true
                };


            using Process process =
                new Process
                {
                    StartInfo = startInfo
                };

            process.Start();

            process.WaitForExit();
        }

        public async Task RestartAsync()
        {
            var timer = Stopwatch.StartNew();

            Debug.WriteLine("Restart: validation started");

            var validation =
                await ValidateConfigurationAsync();

            Debug.WriteLine($"Restart: validation finished ({timer.ElapsedMilliseconds}ms)");


            if (!validation.Success)
            {
                throw new InvalidOperationException(
                    $"Apache configuration is invalid.\n\n{validation.Message}"
                );
            }


            if (IsRunning)
            {
                Debug.WriteLine($"Restart: stopping ({timer.ElapsedMilliseconds}ms)");

                Stop();

                Debug.WriteLine($"Restart: stopped ({timer.ElapsedMilliseconds}ms)");

                await Task.Delay(1000);

                Debug.WriteLine($"Restart: delay complete ({timer.ElapsedMilliseconds}ms)");
            }


            Debug.WriteLine($"Restart: starting ({timer.ElapsedMilliseconds}ms)");

            await StartAsync();

            Debug.WriteLine($"Restart: complete ({timer.ElapsedMilliseconds}ms)");
        }

        public void PrepareVirtualHostCertificate(
    ApacheVirtualHost host)
        {
            if (!host.HttpsEnabled)
                return;


            host.CertificatePath =
                certificateManager.ServerCertificate;


            host.PrivateKeyPath =
                certificateManager.ServerPrivateKey;
        }

        public ApacheSettings GetSettings()
        {
            return new ApacheSettings
            {
                HttpEnabled =
                    IsDirectiveEnabled(
                        File.ReadAllLines(apacheConfig),
                        "Listen 80"
                    ),

                HttpsEnabled =
                    IsHttpsEnabled(),

                RewriteEnabled =
                    IsModuleEnabled(
                        "rewrite_module"
                    ),

                HeadersEnabled =
                    IsModuleEnabled(
                        "headers_module"
                    ),

                DeflateEnabled =
                    IsModuleEnabled(
                        "deflate_module"
                    )
            };
        }

        public void ApplySettings(
    ApacheSettings settings)
        {
            SetListenPortEnabled(
                80,
                settings.HttpEnabled
            );


            if (settings.HttpsEnabled)
            {
                EnableHttps();
            }
            else
            {
                DisableHttps();
            }


            SetModuleEnabled(
                "rewrite_module",
                settings.RewriteEnabled
            );


            SetModuleEnabled(
                "headers_module",
                settings.HeadersEnabled
            );


            SetModuleEnabled(
                "deflate_module",
                settings.DeflateEnabled
            );
        }

        private bool IsModuleEnabled(
    string moduleName)
        {
            if (!File.Exists(apacheConfig))
                return false;


            foreach (string line in File.ReadAllLines(apacheConfig))
            {
                string trimmed =
                    line.Trim();


                if (trimmed.StartsWith("#"))
                    continue;


                if (trimmed.StartsWith(
                    "LoadModule ",
                    StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts =
                        trimmed.Split(
                            ' ',
                            StringSplitOptions.RemoveEmptyEntries
                        );


                    if (parts.Length > 1 &&
                       string.Equals(
                            parts[1],
                            moduleName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        public async Task StartAsync(
            string? documentRoot = null)
        {
            if (IsRunning ||
                Status == ApacheStatus.Starting)
                return;

            SetStatus(
                ApacheStatus.Starting
            );

            if (!File.Exists(apacheExecutable))
            {
                SetStatus(
                    ApacheStatus.Failed
                );

                throw new FileNotFoundException(
                    "Apache server executable was not found.",
                    apacheExecutable
                );
            }

            try
            {
                if (!string.IsNullOrEmpty(documentRoot))
                {
                    SetDocumentRoot(
                        documentRoot
                    );
                }

                ValidateConfiguration();

                apacheProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = apacheExecutable,
                        Arguments =
                            $"-f \"{apacheConfig}\" -d \"{apacheDirectory}\"",
                        WorkingDirectory =
                            apacheDirectory,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                apacheProcess.Start();

                await WaitForServerAsync();

                SetStatus(
                    ApacheStatus.Running
                );
            }
            catch
            {
                SetStatus(
                    ApacheStatus.Failed
                );

                apacheProcess?.Dispose();

                apacheProcess = null;

                throw;
            }
        }

        public void Stop()
        {
            if (Status == ApacheStatus.Stopped)
                return;

            if (Status == ApacheStatus.Stopping)
                return;

            SetStatus(
                ApacheStatus.Stopping
            );

            try
            {
                if (apacheProcess != null &&
                    !apacheProcess.HasExited)
                {
                    apacheProcess.Kill();

                    if (!apacheProcess.WaitForExit(5000))
                    {
                        throw new InvalidOperationException(
                            "Apache did not stop within the expected time."
                        );
                    }
                }

                apacheProcess?.Dispose();

                apacheProcess = null;

                SetStatus(
                    ApacheStatus.Stopped
                );
            }
            catch
            {
                SetStatus(
                    ApacheStatus.Failed
                );

                throw;
            }
        }

        public bool IsHttpsEnabled()
        {
            if (!File.Exists(apacheConfig))
                return false;

            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            bool sslModuleEnabled = false;
            bool socacheModuleEnabled = false;
            bool httpsPortEnabled = false;
            bool sslConfigEnabled = false;

            foreach (string line in lines)
            {
                string trimmed =
                    line.Trim();

                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;

                // A directive beginning with # is disabled.
                if (trimmed.StartsWith("#"))
                    continue;

                if (trimmed.StartsWith(
                    "LoadModule ssl_module ",
                    StringComparison.OrdinalIgnoreCase))
                {
                    sslModuleEnabled = true;
                }

                if (trimmed.StartsWith(
                    "LoadModule socache_shmcb_module ",
                    StringComparison.OrdinalIgnoreCase))
                {
                    socacheModuleEnabled = true;
                }

                if (string.Equals(
                    trimmed,
                    "Listen 443",
                    StringComparison.OrdinalIgnoreCase))
                {
                    httpsPortEnabled = true;
                }

                if (trimmed.StartsWith(
                    "Include ",
                    StringComparison.OrdinalIgnoreCase) &&
                    trimmed.Contains(
                        "httpd-ssl.conf",
                        StringComparison.OrdinalIgnoreCase))
                {
                    sslConfigEnabled = true;
                }
            }

            return
                sslModuleEnabled &&
                socacheModuleEnabled &&
                httpsPortEnabled &&
                sslConfigEnabled;
        }

        public void EnsureConfigurationFile(
    string documentRoot)
        {
            if (File.Exists(apacheConfig))
                return;

            configGenerator.Generate(
                documentRoot
            );
        }

        public void EnableRewriteModule(string apachePath)
        {
            string httpdConf =
                Path.Combine(
                    apachePath,
                    "conf",
                    "httpd.conf"
                );

            if (!File.Exists(httpdConf))
            {
                throw new FileNotFoundException(
                    "Apache configuration file not found.",
                    httpdConf
                );
            }

            string content =
                File.ReadAllText(httpdConf);

            string enabledLine =
                "LoadModule rewrite_module modules/mod_rewrite.so";

            if (File.ReadLines(httpdConf)
                .Any(line =>
                    line.Trim()
                        .Equals(
                            enabledLine,
                            StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            string updatedContent =
                Regex.Replace(
                    content,
                    @"^\s*#\s*LoadModule\s+rewrite_module\s+modules/mod_rewrite\.so",
                    enabledLine,
                    RegexOptions.Multiline |
                    RegexOptions.IgnoreCase
                );

            if (updatedContent == content)
            {
                throw new InvalidOperationException(
                    "Apache rewrite module entry was not found."
                );
            }

            File.WriteAllText(
                httpdConf,
                updatedContent
            );
        }

        public string ReadConfiguration()
        {

            return File.ReadAllText(
                apacheConfig
            );
        }


        public void SaveConfiguration(
            string configuration)
        {
            string backup =
                apacheConfig + ".backup";


            if (File.Exists(apacheConfig))
            {
                File.Copy(
                    apacheConfig,
                    backup,
                    true
                );
            }


            File.WriteAllText(
                apacheConfig,
                configuration
            );


            try
            {
                ValidateConfiguration();
            }
            catch
            {
                if (File.Exists(backup))
                {
                    File.Copy(
                        backup,
                        apacheConfig,
                        true
                    );
                }

                throw;
            }
        }

        public bool IsHttpEnabled()
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            return IsDirectiveEnabled(
                lines,
                "Listen 80"
            );
        }

        public void EnableHttps()
        {
            if (!File.Exists(apacheConfig))
            {
                throw new FileNotFoundException(
                    "Apache configuration file was not found.",
                    apacheConfig
                );
            }

            List<string> domains =
    ReadVirtualHosts()
        .Where(x => !string.IsNullOrWhiteSpace(x.ServerName))
        .Select(x => x.ServerName)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();


            domains.Add("localhost");
            domains.Add("127.0.0.1");
            domains.Add("::1");


            certificateManager.EnsureHttpsCertificate(
                domains
            );

            SetModuleEnabled(
                "ssl_module",
                true
            );

            SetModuleEnabled(
                "socache_shmcb_module",
                true
            );

            SetListenPortEnabled(
                443,
                true
            );

            SetIncludeEnabled(
                "conf/extra/httpd-ssl.conf",
                true
            );
        }

        public void DisableHttps()
        {
            if (!File.Exists(apacheConfig))
            {
                throw new FileNotFoundException(
                    "Apache configuration file was not found.",
                    apacheConfig
                );
            }

            SetModuleEnabled(
                "ssl_module",
                false
            );

            SetModuleEnabled(
                "socache_shmcb_module",
                false
            );

            SetListenPortEnabled(
                443,
                false
            );

            SetIncludeEnabled(
                "conf/extra/httpd-ssl.conf",
                false
            );
        }

        public void SetModuleEnabled(
            string moduleName,
            bool enabled)
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            bool found = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed =
                    lines[i].TrimStart();

                string uncommented =
                    trimmed.TrimStart(
                        '#',
                        ' '
                    );

                if (uncommented.StartsWith(
                    $"LoadModule {moduleName} ",
                    StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] =
                        enabled
                            ? uncommented
                            : "# " + uncommented;

                    found = true;

                    break;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException(
                    $"Apache module '{moduleName}' was not found in the configuration."
                );
            }

            File.WriteAllLines(
                apacheConfig,
                lines
            );
        }

        public void SetModuleEnabled(
    ApacheModule module,
    bool enabled)
        {
            if (enabled)
            {
                EnableDependencies(module);
            }

            SetModuleEnabled(
                module.Directive,
                enabled
            );
        }

        private void EnableDependencies(
    ApacheModule module)
        {
            foreach (string dependency in module.Dependencies)
            {
                SetModuleEnabled(
                    dependency,
                    true
                );
            }
        }

        private void SetListenPortEnabled(
            int port,
            bool enabled)
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            bool found = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed =
                    lines[i].TrimStart();

                if (string.Equals(
                    trimmed,
                    $"Listen {port}",
                    StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        trimmed,
                        $"# Listen {port}",
                        StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] =
                        enabled
                            ? $"Listen {port}"
                            : $"# Listen {port}";

                    found = true;

                    break;
                }
            }

            if (!found &&
                enabled)
            {
                List<string> updatedLines =
                    lines.ToList();

                int listenIndex =
                    -1;

                for (int i = 0; i < updatedLines.Count; i++)
                {
                    string trimmed =
                        updatedLines[i].TrimStart();

                    if (trimmed.StartsWith(
                        "Listen ",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        listenIndex = i;
                    }
                }

                if (listenIndex >= 0)
                {
                    updatedLines.Insert(
                        listenIndex + 1,
                        $"Listen {port}"
                    );
                }
                else
                {
                    updatedLines.Add(
                        $"Listen {port}"
                    );
                }

                lines =
                    updatedLines.ToArray();
            }

            if (!found &&
                !enabled)
            {
                return;
            }

            File.WriteAllLines(
                apacheConfig,
                lines
            );
        }

        private void SetIncludeEnabled(
            string includePath,
            bool enabled)
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            bool found = false;

            string[] possibleDirectives =
            {
                $"Include {includePath}",
                $"# Include {includePath}",
                $"Include \"${{SRVROOT}}/{includePath}\"",
                $"# Include \"${{SRVROOT}}/{includePath}\""
            };

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed =
                    lines[i].Trim();

                if (possibleDirectives.Any(
                    directive =>
                        string.Equals(
                            trimmed,
                            directive,
                            StringComparison.OrdinalIgnoreCase
                        )))
                {
                    lines[i] =
                        enabled
                            ? $"Include \"${{SRVROOT}}/{includePath}\""
                            : $"# Include \"${{SRVROOT}}/{includePath}\"";

                    found = true;

                    break;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException(
                    $"Apache configuration does not contain the include directive '{includePath}'."
                );
            }

            File.WriteAllLines(
                apacheConfig,
                lines
            );
        }

        private bool IsDirectiveEnabled(
            string[] lines,
            string directive)
        {
            foreach (string line in lines)
            {
                string trimmed =
                    line.Trim();

                if (string.Equals(
                    trimmed,
                    directive,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetDocumentRoot(
            string documentRoot)
        {
            if (!Directory.Exists(documentRoot))
            {
                throw new DirectoryNotFoundException(
                    $"The Apache document root does not exist.\n\n{documentRoot}"
                );
            }

            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            bool documentRootUpdated = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed =
                    lines[i].TrimStart();

                if (trimmed.StartsWith(
                    "DocumentRoot ",
                    StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] =
                        $"DocumentRoot \"{documentRoot}\"";

                    documentRootUpdated = true;

                    break;
                }
            }

            if (!documentRootUpdated)
            {
                throw new InvalidOperationException(
                    "The Apache configuration does not contain a DocumentRoot directive."
                );
            }

            File.WriteAllLines(
                apacheConfig,
                lines
            );
        }

        private async Task WaitForServerAsync()
        {
            const int timeout = 30;
            const int delay = 500;

            bool httpsEnabled =
                IsHttpsEnabled();

            for (int i = 0; i < timeout * 1000 / delay; i++)
            {
                if (apacheProcess != null &&
                    apacheProcess.HasExited)
                {
                    string error =
                        await apacheProcess
                            .StandardError
                            .ReadToEndAsync();

                    string output =
                        await apacheProcess
                            .StandardOutput
                            .ReadToEndAsync();

                    string message =
                        string.IsNullOrWhiteSpace(error)
                            ? output
                            : error;

                    throw new InvalidOperationException(
                        $"Apache stopped unexpectedly while starting.\n\n{message}"
                    );
                }

                bool httpReady =
                    await IsPortOpenAsync(
                        apachePort
                    );

                if (!httpReady)
                {
                    await Task.Delay(
                        delay
                    );

                    continue;
                }

                if (httpsEnabled)
                {
                    bool httpsReady =
                        await IsPortOpenAsync(
                            httpsPort
                        );

                    if (!httpsReady)
                    {
                        await Task.Delay(
                            delay
                        );

                        continue;
                    }
                }

                return;
            }

            throw new TimeoutException(
                httpsEnabled
                    ? "Apache did not become ready on HTTP and HTTPS within the expected time."
                    : "Apache did not become ready within the expected time."
            );
        }

        public async Task<(bool Success, string Message)> ValidateConfigurationAsync()
        {
            ProcessStartInfo psi = new()
            {
                FileName = apacheExecutable,

                Arguments = "-t",

                RedirectStandardOutput = true,
                RedirectStandardError = true,

                UseShellExecute = false,
                CreateNoWindow = true
            };


            using Process process = new()
            {
                StartInfo = psi
            };


            process.Start();


            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();


            await process.WaitForExitAsync();


            string message =
                string.IsNullOrWhiteSpace(error)
                    ? output
                    : error;


            return (
                process.ExitCode == 0,
                message.Trim()
            );
        }

        private async Task<bool> IsPortOpenAsync(
            int port)
        {
            try
            {
                using TcpClient client =
                    new TcpClient();

                await client.ConnectAsync(
                    "127.0.0.1",
                    port
                );

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private void SetStatus(
            ApacheStatus status)
        {
            if (Status == status)
                return;

            Status = status;

            StatusChanged?.Invoke(
                this,
                EventArgs.Empty
            );
        }

        public void ValidateConfiguration()
        {
            ProcessStartInfo startInfo =
                new ProcessStartInfo
                {
                    FileName = apacheExecutable,
                    Arguments =
                        $"-t -f \"{apacheConfig}\"",
                    WorkingDirectory =
                        apacheDirectory,
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
                    $"Apache configuration validation failed.\n\n{message}"
                );
            }
        }

        private string CreateDomainName(string name)
        {
            return Regex.Replace(
                name.ToLower().Trim(),
                @"[^a-z0-9\-]",
                "-"
            )
            .Trim('-');
        }

        public async Task CreateProjectVirtualHost(
            string projectName,
            string projectPath,
            string domain)
        {
            if (!Directory.Exists(projectPath))
            {
                throw new DirectoryNotFoundException(
                    $"Virtual host directory does not exist: {projectPath}"
                );
            }

            ApacheVirtualHost host =
                new ApacheVirtualHost
                {
                    ServerName = domain,
                    DocumentRoot = projectPath,
                    Directory = projectPath,
                    AllowOverride = "All",
                    ErrorLog = $"logs/{domain}-error.log",
                    CustomLog = $"logs/{domain}-access.log",
                    HttpsEnabled = IsHttpsEnabled()
                };

            List<ApacheVirtualHost> hosts =
                ReadVirtualHosts();

            hosts.RemoveAll(
                x => x.ServerName.Equals(
                    domain,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            hosts.Add(host);

            await SaveVirtualHosts(hosts);
        }
    }
}