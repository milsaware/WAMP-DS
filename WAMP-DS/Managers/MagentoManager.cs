using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using WAMP_DS.Core;
using WAMP_DS.Models;
using WAMP_DS.Views;

namespace WAMP_DS.Managers
{
    public class MagentoManager
    {
        private readonly string composerPath;
        private readonly string phpPath;
        private readonly MagentoCredentialManager credentialManager;
        private readonly InstallationPaths installationPaths;
        private readonly PhpSettingsManager phpSettingsManager;
        private readonly PhpSettingsManager phpSettings;
        private readonly MagentoRequirementManager requirementManager;
        private readonly ApacheManager apacheManager;
        private readonly DatabaseManager databaseManager;
        private readonly MySQLSettingsManager mySqlSettingsManager;

        public event EventHandler<string>? OutputReceived;

        public MagentoManager(InstallationPaths installationPaths)
        {
            this.installationPaths = installationPaths;

            composerPath = Path.Combine(
                AppContext.BaseDirectory,
                "tools",
                "composer",
                "composer.phar"
            );

            phpPath = Path.Combine(
                installationPaths.PhpPath,
                "php.exe"
            );

            mySqlSettingsManager = new MySQLSettingsManager();

            databaseManager = new DatabaseManager(
                mySqlSettingsManager
            );

            credentialManager = new MagentoCredentialManager();

            phpSettingsManager = new PhpSettingsManager(
                installationPaths.PhpPath
            );

            phpSettings = new PhpSettingsManager(
                Path.GetDirectoryName(phpPath)!
            );

            requirementManager = new MagentoRequirementManager(
                phpSettingsManager
            );
            
            apacheManager = new ApacheManager(
                installationPaths
            );
        }

        public async Task<bool> CompleteInstallation(
            string projectPath,
            string databaseName,
            MagentoInstallSettings? installSettings
        )
        {
            Report("Creating Magento database...");

            await databaseManager.CreateDatabase(
                databaseName
            );

            Report("Magento database created.");

            Report("Running Magento installer...");

            if (installSettings == null)
            {
                Report("Magento settings are missing.");

                return false;
            }

            bool installed = await RunMagentoInstaller(
                projectPath,
                databaseName,
                installSettings,
                mySqlSettingsManager.Settings
            );

            if (!installed)
            {
                Report("Magento installation failed.");

                return false;
            }

            return true;
        }

        private string? CreateComposerAuth()
        {
            var credentials = credentialManager.Load();

            if (string.IsNullOrWhiteSpace(credentials.PublicKey) ||
                string.IsNullOrWhiteSpace(credentials.PrivateKey))
            {
                Report("ERROR: Magento authentication keys are missing.");

                return null;
            }

            string composerDirectory = Path.GetDirectoryName(
                composerPath
            )!;

            string authPath = Path.Combine(
                composerDirectory,
                "auth.json"
            );

            var auth =
            new Dictionary<string, object>
            {
                {
                    "http-basic",
                    new Dictionary<string, object>
                    {
                        {
                            "repo.magento.com",
                            new Dictionary<string, string>
                            {
                                {
                                    "username",
                                    credentials.PublicKey
                                },
                                {
                                    "password",
                                    credentials.PrivateKey
                                }
                            }
                        }
                    }
                }
            };

            string json = JsonSerializer.Serialize(
                auth,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }
            )
            .Replace(
                "repo_magento_com",
                "repo.magento.com"
            );

            System.IO.File.WriteAllText(
                authPath,
                json
            );

            Report("Magento authentication configured.");

            return json;
        }

        private void EnableMagentoSetup(string projectPath)
        {
            string setupHtaccess = Path.Combine(
                projectPath,
                "setup",
                ".htaccess"
            );

            if (!File.Exists(setupHtaccess))
            {
                Report("Magento setup .htaccess not found.");

                return;
            }

            string content = File.ReadAllText(
                setupHtaccess
            );

            string updatedContent = Regex.Replace(
               content,
               @"<Files ""index\.php"">.*?</Files>",
               @"<Files ""index.php"">
                    Require all granted
               </Files>",
               RegexOptions.Singleline
            );

            if (updatedContent == content)
            {
                Report("Magento setup .htaccess pattern not found.");

                return;
            }

            File.WriteAllText(
                setupHtaccess,
                updatedContent
            );

            Report("Magento setup access enabled.");
        }

        private string ExtractAdminUri(string url)
        {
            Uri uri = new(url);

            string path = uri.AbsolutePath.Trim('/');

            return string.IsNullOrWhiteSpace(path)
                ? "admin"
                : path;
        }

        private string ExtractBaseUrl(string url)
        {
            Uri uri = new Uri(url);

            return uri.Host;
        }

        public string GetDocumentRoot(string projectPath)
        {
            Report("Setting Magento document root...");

            string documentRoot = Path.Combine(
                projectPath,
                "pub"
            );

            if (!Directory.Exists(documentRoot))
            {
                throw new DirectoryNotFoundException(
                    "Magento pub folder missing."
                );
            }

            return documentRoot;
        }

        public MagentoInstallSettings? GetInstallationSettings(string virtualHostDomain)
        {
            MagentoSettingsWindow settingsWindow = new MagentoSettingsWindow(
                $"https://{virtualHostDomain}/admin"
            )
            {
                Owner = Application.Current.MainWindow
            };

            if (settingsWindow.ShowDialog() != true)
            {
                Report("Magento installation cancelled.");

                return null;
            }

            MagentoInstallSettings? installSettings = settingsWindow.Settings;

            if (!HasCredentials())
            {
                MagentoCredentialsWindow window = new MagentoCredentialsWindow();

                if (window.ShowDialog() != true)
                {
                    Report("Magento authentication cancelled.");

                    return null;
                }
            }

            return installSettings;
        }

        public bool HasCredentials()
        {
            MagentoSettings credentials = credentialManager.Load();

            return
                !string.IsNullOrWhiteSpace(credentials.PublicKey) &&
                !string.IsNullOrWhiteSpace(credentials.PrivateKey);
        }

        public async Task<string?> InstallMagento(
            string location,
            string projectName,
            MagentoInstallSettings installSettings)
        {
            Report($"Collected the following settings:");
            Report($"Admin Username: {installSettings.AdminUsername}");
            Report($"Admin Email: {installSettings.AdminEmail}");
            Report($"Admin URL: {installSettings.AdminUrl}");
            Report($"Admin First Name: {installSettings.AdminFirstName}");
            Report($"Admin Last Name: {installSettings.AdminLastName}");
            Report($"Language: {installSettings.Language}");
            Report($"Timezone: {installSettings.Timezone}");
            Report($"Currency: {installSettings.Currency}");
            Report($"Magento settings configured");

            string projectPath = Path.Combine(
                location,
                projectName
            );

            Report("Building Magento project...");
            Report($"Project location: {projectPath}");

            if (!System.IO.File.Exists(composerPath))
            {
                Report("ERROR: Composer was not found.");

                return null;
            }

            if (!System.IO.File.Exists(phpPath))
            {
                Report("ERROR: PHP executable was not found.");

                return null;
            }

            Report("Checking Magento PHP requirements...");

            requirementManager.Prepare();

            Report("Magento PHP requirements prepared.");

            PrepareMagentoRequirements();

            string? composerAuth = CreateComposerAuth();

            if (composerAuth == null)
            {
                return null;
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = phpPath,

                Arguments = $"\"{composerPath}\" create-project --no-interaction --repository-url=https://repo.magento.com/ magento/project-community-edition \"{projectPath}\"",

                WorkingDirectory = location,

                RedirectStandardOutput = true,

                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.EnvironmentVariables["COMPOSER_AUTH"] = composerAuth;

            using Process process =
                new()
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Report(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Report(e.Data);
                }
            };

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                Report($"Failed launching Composer: {ex.Message}");

                return null;
            }

            process.BeginOutputReadLine();

            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            Report($"Composer exited with code {process.ExitCode}");

            if (process.ExitCode != 0)
            {
                Report("Magento installation failed.");

                return null;
            }

            if (!Directory.Exists(projectPath))
            {
                Report("ERROR: Magento directory was not created.");

                return null;
            }

            EnableMagentoSetup(projectPath);

            Report("Magento project created successfully.");

            return projectPath;
        }

        private void PrepareMagentoRequirements()
        {
            Report("Checking Magento PHP requirements...");

            string[] requiredExtensions =
            {
                "fileinfo",
                "ftp",
                "openssl",
                "xsl",
                "intl",
                "zip",
                "curl",
                "gd",
                "soap",
                "sodium",
                "mbstring",
                "mysqli",
                "pdo_mysql",
                "sockets"
            };

            foreach (string extension in requiredExtensions)
            {
                phpSettings.EnableExtension(
                    extension
                );

                Report($"Enabled PHP extension: {extension}");
            }

            phpSettings.SetValue(
                "PHP",
                "memory_limit",
                "2G"
            );

            Report("PHP memory limit set: 2G");

            phpSettings.Save();

            Report("PHP configuration saved.");

            // Apache configuration
            apacheManager.EnableRewriteModule(
                installationPaths.ApachePath
            );

            Report("Apache mod_rewrite enabled.");
            Report("Magento PHP requirements prepared.");
        }

        private void Report(string message)
        {
            OutputReceived?.Invoke(
                this,
                message
            );
        }

        private async Task<bool> RunMagentoCommand(
            string projectPath,
            string arguments)
        {
            int silenceWarningStage = 0;

            Report($"Running Magento command: {arguments}");

            DateTime lastOutput = DateTime.Now;
            Timer? silenceTimer = null;

            ProcessStartInfo startInfo = new()
            {
                FileName = phpPath,

                Arguments = $"\"{Path.Combine(projectPath, "bin", "magento")}\" {arguments}",

                WorkingDirectory = projectPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process process = new()
            {
                StartInfo = startInfo
            };

            void ResetOutputTimer()
            {
                lastOutput = DateTime.Now;
            }

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    ResetOutputTimer();

                    Report(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    ResetOutputTimer();

                    Report(e.Data);
                }
            };

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                Report($"Failed running Magento command: {ex.Message}");

                return false;
            }

            silenceTimer = new Timer(_ =>
            {
                double silentSeconds = (DateTime.Now - lastOutput).TotalSeconds;

                if (silentSeconds >= 90)
                {
                    switch (silenceWarningStage)
                    {
                        case 0:
                            Report("Magento is still working. This step can take several minutes without output. WAMP-DS is running normally — please do not close it.");

                            break;

                        case 1:
                            Report("Magento is still busy. It hasn't forgotten about you — it is just processing a lot of files. WAMP-DS is still running normally.");

                            break;

                        case 2:
                            Report("Still working away. Magento is probably having a deep think about life, indexes, and thousands of generated files. ☕");

                            break;

                        case 3:
                            Report("Magento is taking its time. This is normal during heavy compilation steps. WAMP-DS is patiently standing by with snacks.");

                            break;
                    }

                    silenceWarningStage++;

                    if (silenceWarningStage > 3)
                    {
                        silenceWarningStage = 0;
                    }

                    lastOutput = DateTime.Now;
                }

            },
            null,
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10)
            );

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            silenceTimer.Dispose();

            if (process.ExitCode != 0)
            {
                Report($"Magento command failed with exit code {process.ExitCode}");

                return false;
            }

            Report($"Magento command completed successfully: {arguments}");

            return true;
        }

        public async Task<bool> RunMagentoInstaller(
            string projectPath,
            string databaseName,
            MagentoInstallSettings settings,
            MySQLSettings mysqlSettings)
        {
            Report("Running Magento setup installer...");

            MagentoPatchManager patchManager = new MagentoPatchManager();

            patchManager.PatchGd2(projectPath);

            Report("Magento GD2 Windows patch applied.");

            patchManager.PatchPluginListGenerator(projectPath);

            Report("Magento PluginListGenerator Windows patch applied.");

            patchManager.PatchTemplateValidator(projectPath);

            Report("Magento Template Validator Windows patch applied.");

            patchManager.PatchStaticResource(projectPath);

            Report("Magento StaticResource Windows patch applied.");

            ProcessStartInfo startInfo = new()
            {
                FileName = phpPath,

                Arguments =
                    $"\"{Path.Combine(projectPath, "bin", "magento")}\" setup:install " +
                    $"--base-url=https://{ExtractBaseUrl(settings.AdminUrl)} " +
                    $"--db-host={mysqlSettings.Host} " +
                    $"--db-name={databaseName} " +
                    $"--db-user={mysqlSettings.Username} " +
                    $"--db-password={mysqlSettings.Password} " +
                    $"--admin-user={settings.AdminUsername} " +
                    $"--admin-password={settings.AdminPassword} " +
                    $"--admin-email={settings.AdminEmail} " +
                    $"--admin-firstname={settings.AdminFirstName} " +
                    $"--admin-lastname={settings.AdminLastName} " +
                    $"--backend-frontname={ExtractAdminUri(settings.AdminUrl)} " +
                    $"--language={settings.Language} " +
                    $"--timezone={settings.Timezone} " +
                    $"--currency={settings.Currency} " +
                    $"--search-engine=opensearch " +
                    $"--opensearch-host=127.0.0.1 " +
                    $"--opensearch-port=9200",

                    WorkingDirectory = projectPath,

                    UseShellExecute = false,

                    CreateNoWindow = true,

                    RedirectStandardOutput = true,

                    RedirectStandardError = true
                };

            using Process process = new()
            {
                StartInfo = startInfo
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Report(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Report(e.Data);
                }
            };

            process.Start();

            process.BeginOutputReadLine();

            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                Report("Magento setup failed.");

                return false;
            }

            Report("Magento setup completed successfully.");

            bool developerMode = await RunMagentoCommand(
                projectPath,
                "deploy:mode:set developer"
            );

            if (!developerMode)
            {
                return false;
            }

            bool staticContent = await RunMagentoCommand(
                projectPath,
                $"setup:static-content:deploy -f {settings.Language}"
            );

            if (!staticContent)
            {
                return false;
            }

            patchManager.PatchStaticContentSourceMaps(
                projectPath
            );

            Report("Magento static content source maps patched.");

            bool compile = await RunMagentoCommand(
                    projectPath,
                    "setup:di:compile"
                );

            if (!compile)
            {
                return false;
            }

            patchManager.PatchContactLayout(
                projectPath
            );

            Report("Magento Contact layout patch applied.");

            Report("Removing Magento generated files...");

            var generatedPath = Path.Combine(projectPath, "generated");

            try
            {
                if (Directory.Exists(generatedPath))
                {
                    Report("Generated folder found. Deleting...");

                    Directory.Delete(
                        generatedPath,
                        true
                    );

                    await Task.Delay(500);

                    if (Directory.Exists(generatedPath))
                    {
                        Report("ERROR: Generated folder still exists after deletion.");
                        return false;
                    }

                    Report("Generated folder deleted successfully.");
                }
                else
                {
                    Report("Generated folder does not exist.");
                }
            }
            catch (Exception ex)
            {
                Report($"Failed to remove generated folder: {ex.Message}");
                return false;
            }

            bool disableTwoFactor = await RunMagentoCommand(
                projectPath,
                "module:disable Magento_AdminAdobeImsTwoFactorAuth Magento_TwoFactorAuth"
            );

            if (!disableTwoFactor)
            {
                Report("Magento Two Factor Auth disable failed.");

                return false;
            }

            Report("Magento Two Factor Authentication disabled.");

            bool upgrade = await RunMagentoCommand(
                projectPath,
                "setup:upgrade"
            );

            if (!upgrade)
            {
                Report("Magento setup upgrade failed.");

                return false;
            }

            Report("Magento setup upgrade completed.");

            bool cache = await RunMagentoCommand(
                projectPath,
                "cache:flush"
            );

            if (!cache)
            {
                return false;
            }

            Report("Magento post-install preparation completed.");

            return true;
        }
    }
}