using System.Diagnostics;
using System.IO;
using WAMP_DS.Core;
using WAMP_DS.Managers;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class CodeIgniterManager
    {
        private readonly string composerPath;
        private readonly string phpPath;
        private readonly PhpIniManager phpIniManager;
        private readonly DatabaseManager databaseManager;

        public event EventHandler<string>? OutputReceived;

        public CodeIgniterManager(
            InstallationPaths installationPaths,
            DatabaseManager databaseManager
        )
        {
            this.databaseManager = databaseManager;

            composerPath = Path.Combine(
                AppContext.BaseDirectory,
                "tools",
                "composer",
                "composer.phar"
            );

            phpPath = Path.Combine(
                AppContext.BaseDirectory,
                "runtimes",
                "php",
                "8.5.8",
                "php.exe"
            );

            phpIniManager = new PhpIniManager();
        }

        private void ConfigureEnvironment(
            string projectPath,
            ProjectCreationOptions options
        )
        {
            Report("Configuring CodeIgniter environment...");

            string exampleEnv = Path.Combine(
                projectPath,
                "env"
            );

            string envFile = Path.Combine(
                projectPath,
                ".env"
            );

            if (File.Exists(exampleEnv))
            {
                File.Copy(
                    exampleEnv,
                    envFile,
                    true
                );

                Report(".env file created.");
            }
            else
            {
                Report("CodeIgniter env template not found.");

                return;
            }

            string[] lines = File.ReadAllLines(envFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(
                    "# CI_ENVIRONMENT"))
                {
                    lines[i] = "CI_ENVIRONMENT = development";
                }

                if (lines[i].StartsWith("# app.baseURL"))
                {
                    lines[i] = $"app.baseURL = 'http://{options.VirtualHostDomain}/'";
                }
            }

            File.WriteAllLines(
                envFile,
                lines
            );

            Report("Development environment enabled.");
        }

        private void ConfigureDatabase(
            string projectPath,
            ProjectCreationOptions options
        )
        {
            string envFile = Path.Combine(
                projectPath,
                ".env"
            );

            if (!File.Exists(envFile))
                return;

            string[] lines = File.ReadAllLines(envFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("# database.default.hostname"))
                {
                    lines[i] = "database.default.hostname = localhost";
                }

                if (lines[i].StartsWith("# database.default.database"))
                {
                    lines[i] = $"database.default.database = {options.DatabaseName}";
                }

                if (lines[i].StartsWith("# database.default.username"))
                {
                    lines[i] = "database.default.username = root";
                }

                if (lines[i].StartsWith("# database.default.password"))
                {
                    lines[i] = "database.default.password = ";
                }

                if (lines[i].StartsWith("# database.default.DBDriver"))
                {
                    lines[i] = "database.default.DBDriver = MySQLi";
                }
            }

            File.WriteAllLines(
                envFile,
                lines
            );

            Report("Database configuration added.");
        }

        private void ConfigurePhp()
        {
            Report("Configuring PHP extensions...");

            string[] extensions =
            {
                "intl",
                "mbstring",
                "curl",
                "fileinfo",
                "openssl",
                "zip",
                "pdo_mysql",
                "pdo_sqlite",
                "sqlite3"
            };

            foreach (string extension in extensions)
            {
                phpIniManager.EnableExtension(extension);

                Report(
                    $"Enabled PHP extension: {extension}"
                );
            }
        }

        public async Task<bool> InstallCodeIgniter(
            string projectPath,
            ProjectCreationOptions options
        )
        {
            try
            {
                Report("Building CodeIgniter project...");
                Report($"Project location: {projectPath}");

                if (!File.Exists(composerPath))
                {
                    Report("ERROR: Composer was not found.");

                    return false;
                }

                if (!File.Exists(phpPath))
                {
                    Report("ERROR: PHP executable was not found.");

                    return false;
                }

                Report("Composer found.");
                Report("PHP found.");
                Report("Preparing PHP environment...");

                ConfigurePhp();

                Report("Starting Composer installer...");

                string parentDirectory = Directory.GetParent(projectPath)!.FullName;

                ProcessStartInfo startInfo = new()
                {
                    FileName = phpPath,

                    Arguments =
                        $"\"{composerPath}\" create-project codeigniter4/appstarter \"{Path.GetFullPath(projectPath)}\" --no-interaction",

                    WorkingDirectory = parentDirectory,

                    RedirectStandardOutput = true,
                    RedirectStandardError = true,

                    UseShellExecute = false,

                    CreateNoWindow = true
                };

                using Process process = new()
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
                    Report(
                        $"Failed launching Composer: {ex.Message}"
                    );

                    return false;
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                Report("Downloading CodeIgniter dependencies...");

                await process.WaitForExitAsync();

                Report($"Composer exited with code {process.ExitCode}");

                if (process.ExitCode != 0)
                {
                    Report(
                        "CodeIgniter installation failed."
                    );

                    return false;
                }

                if (!Directory.Exists(projectPath))
                {
                    Report("ERROR: CodeIgniter directory was not created.");

                    return false;
                }

                Report("CodeIgniter project created successfully.");

                ConfigureEnvironment(
                    projectPath,
                    options
                );

                if (options.CreateDatabase)
                {
                    Report("Creating MySQL database...");

                    await databaseManager.CreateDatabase(
                        options.DatabaseName
                    );

                    Report("Database created.");

                    ConfigureDatabase(
                        projectPath,
                        options
                    );
                }

                return true;
            }
            catch (Exception ex)
            {
                Report(
                    $"CodeIgniter installation failed: {ex.Message}"
                );

                return false;
            }
        }

        private void Report(string message)
        {
            OutputReceived?.Invoke(
                this,
                message
            );
        }
    }
}