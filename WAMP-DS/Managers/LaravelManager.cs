using System.Diagnostics;
using System.IO;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class LaravelManager
    {
        private readonly string composerPath;
        private readonly string phpPath;
        private readonly PhpIniManager phpIniManager;
        private readonly DatabaseManager databaseManager;
        private readonly MySQLSettings mysqlSettings;

        public event EventHandler<string>? OutputReceived;

        public LaravelManager(
            DatabaseManager databaseManager,
            MySQLSettingsManager mysqlSettingsManager
        )
        {
            this.databaseManager = databaseManager;
            this.mysqlSettings = mysqlSettingsManager.Settings;

            composerPath = Path.Combine(
                AppContext.BaseDirectory,
                "tools",
                "composer",
                "composer.phar"
            );

            phpIniManager = new PhpIniManager();

            phpPath = Path.Combine(
                AppContext.BaseDirectory,
                "runtimes",
                "php",
                "8.5.8",
                "php.exe"
            );
        }

        private void ConfigureEnvironment(
            string projectPath,
            ProjectCreationOptions options
        )
        {
            string envFile = Path.Combine(
                projectPath,
                ".env"
            );

            if (!File.Exists(envFile))
            {
                Report(".env file missing.");
                return;
            }

            string[] lines = File.ReadAllLines(envFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("APP_URL="))
                {
                    lines[i] = $"APP_URL=http://{options.VirtualHostDomain}";
                }

                if (lines[i].StartsWith("DB_CONNECTION="))
                {
                    lines[i] = "DB_CONNECTION=mysql";
                }

                if (lines[i].StartsWith("# DB_HOST="))
                {
                    lines[i] = "DB_HOST=127.0.0.1";
                }

                if (lines[i].StartsWith("# DB_PORT="))
                {
                    lines[i] = "DB_PORT=3306";
                }

                if (lines[i].StartsWith("# DB_DATABASE="))
                {
                    lines[i] = $"DB_DATABASE={options.DatabaseName}";
                }

                if (lines[i].StartsWith("# DB_USERNAME="))
                {
                    lines[i] = $"DB_USERNAME={mysqlSettings.Username}";
                }

                if (lines[i].StartsWith("# DB_PASSWORD="))
                {
                    lines[i] = $"DB_PASSWORD={mysqlSettings.Password}";
                }
            }

            File.WriteAllLines(
                envFile,
                lines
            );

            Report("Laravel environment configured.");
        }

        private void ConfigurePhp()
        {
            Report("Configuring PHP extensions...");

            string[] extensions =
            {
                "curl",
                "fileinfo",
                "mbstring",
                "openssl",
                "pdo_mysql",
                "pdo_sqlite",
                "sqlite3",
                "zip",
                "intl",
                "gd",
                "bcmath"
            };

            foreach (string extension in extensions)
            {
                phpIniManager.EnableExtension(extension);

                Report(
                    $"Enabled PHP extension: {extension}"
                );
            }
        }

        public async Task<string?> InstallLaravel(
            string location,
            string projectName,
            ProjectCreationOptions options)
        {
            string projectPath = Path.Combine(
                location,
                projectName
            );

            Report("Building Laravel project...");
            Report($"Project location: {projectPath}");

            if (!File.Exists(composerPath))
            {
                Report("ERROR: Composer was not found.");

                throw new FileNotFoundException(
                    "Composer executable missing.",
                    composerPath
                );
            }

            if (!File.Exists(phpPath))
            {
                Report("ERROR: PHP executable was not found.");

                throw new FileNotFoundException(
                    "PHP executable missing.",
                    phpPath
                );
            }

            Report("Composer found.");
            Report("PHP found.");
            Report("Preparing PHP environment...");

            ConfigurePhp();

            Report("Starting Composer installer...");

            ProcessStartInfo startInfo = new()
            {
                FileName = phpPath,

                Arguments =
                    $"\"{composerPath}\" create-project laravel/laravel \"{Path.GetFullPath(projectPath)}\" --no-interaction",

                WorkingDirectory = location,

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

                throw;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            Report("Downloading Laravel dependencies...");

            await process.WaitForExitAsync();

            Report($"Composer exited with code {process.ExitCode}");

            if (process.ExitCode != 0)
            {
                Report("Laravel installation failed.");

                throw new Exception(
                    "Laravel installation failed."
                );
            }

            if (!Directory.Exists(projectPath))
            {
                Report("ERROR: Laravel directory was not created.");

                return null;
            }

            Report("Laravel project created successfully.");

            ConfigureEnvironment(
                projectPath,
                options
            );

            if (options.CreateDatabase)
            {
                Report("Creating Laravel database...");

                await databaseManager.CreateDatabase(
                    options.DatabaseName
                );

                Report("Laravel database created.");
            }

            await RunLaravelCommand(
                "artisan migrate --force",
                projectPath
            );

            return projectPath;
        }

        private void Report(string message)
        {
            OutputReceived?.Invoke(
                this,
                message
            );
        }        

        private async Task<bool> RunLaravelCommand(
            string arguments,
            string workingDirectory)
        {
            Report($"Running Laravel command: php {arguments}");

            ProcessStartInfo startInfo = new()
            {
                FileName = phpPath,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
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

            return process.ExitCode == 0;
        }
    }
}