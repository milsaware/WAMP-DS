using System.IO;
using System.IO.Compression;
using System.Net.Http;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public class PhpInstaller : IInstaller
    {
        public string Name =>
            "PHP";

        public string Version =>
            "8.5.8";

        private const string DownloadUrl =
            "https://downloads.php.net/~windows/releases/archives/php-8.5.8-Win32-vs17-x64.zip";

        public async Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            string phpRoot =
                Path.Combine(
                    installationPath,
                    "runtimes",
                    "php",
                    Version);

            string tempDirectory =
                Path.Combine(
                    Path.GetTempPath(),
                    "WAMP-DS",
                    "php");

            string zipPath =
                Path.Combine(
                    tempDirectory,
                    "php.zip");

            progress?.Report(
                new InstallationProgress
                {
                    Message = $"Preparing PHP {Version}...",
                    Percentage = 0
                });

            Directory.CreateDirectory(phpRoot);
            Directory.CreateDirectory(tempDirectory);

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message = $"Downloading PHP {Version}...",
                    Percentage = 10
                });

            using HttpClient client = new();

            using HttpResponseMessage response =
                await client.GetAsync(
                    DownloadUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

            response.EnsureSuccessStatusCode();

            long? totalBytes =
                response.Content.Headers.ContentLength;

            byte[] buffer =
                new byte[81920];

            long totalRead = 0;

            using (Stream input =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken))
            using (FileStream output =
                new(
                    zipPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None))
            {
                int bytesRead;

                while (
                    (bytesRead =
                        await input.ReadAsync(
                            buffer,
                            cancellationToken)) > 0)
                {
                    await output.WriteAsync(
                        buffer.AsMemory(
                            0,
                            bytesRead),
                        cancellationToken);

                    totalRead += bytesRead;

                    if (totalBytes.HasValue &&
                        totalBytes.Value > 0)
                    {
                        double downloadPercentage =
                            (double)totalRead /
                            totalBytes.Value *
                            70;

                        progress?.Report(
                            new InstallationProgress
                            {
                                Message = "",
                                Percentage =
                                    10 +
                                    downloadPercentage
                            });
                    }
                }
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message = $"Extracting PHP {Version}...",
                    Percentage = 80
                });

            if (Directory.Exists(phpRoot))
            {
                Directory.Delete(
                    phpRoot,
                    true);
            }

            Directory.CreateDirectory(
                phpRoot);

            await Task.Run(
                () =>
                    ZipFile.ExtractToDirectory(
                        zipPath,
                        phpRoot),
                cancellationToken);

            progress?.Report(
                new InstallationProgress
                {
                    Message = $"Verifying PHP {Version}...",
                    Percentage = 95
                });

            string phpExecutable =
                Path.Combine(
                    phpRoot,
                    "php.exe");

            if (!File.Exists(phpExecutable))
            {
                throw new InvalidOperationException(
                    $"PHP installation failed. " +
                    $"The expected file was not found: {phpExecutable}");
            }

            ConfigurePHP(phpRoot);

            progress?.Report(
                new InstallationProgress
                {
                    Message = $"PHP {Version} installed.",
                    Percentage = 100
                });

            try
            {
                File.Delete(zipPath);
            }
            catch
            {
                // Temporary download can be cleaned up later.
            }
        }

        private void ConfigurePHP(string phpRoot)
        {
            string templatePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    "PHP",
                    "php.ini");

            string configPath =
                Path.Combine(
                    phpRoot,
                    "php.ini");

            string config =
                File.ReadAllText(templatePath);

            config = config.Replace(
                "{{PHPROOT}}",
                phpRoot);

            string tempRoot =
                Path.Combine(
                    phpRoot,
                    "tmp");

            string logsRoot =
                Path.Combine(
                    phpRoot,
                    "logs");

            Directory.CreateDirectory(
                tempRoot);

            config = config.Replace(
                "{{TEMPROOT}}",
                tempRoot);

            Directory.CreateDirectory(
                logsRoot);

            config = config.Replace(
                "{{LOGSROOT}}",
                logsRoot);

            File.WriteAllText(
                configPath,
                config);
        }
    }
}