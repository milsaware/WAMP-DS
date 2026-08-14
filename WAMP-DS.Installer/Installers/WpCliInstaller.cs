using System.IO;
using System.Net.Http;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public class WpCliInstaller : IInstaller
    {
        public string Name =>
            "WP-CLI";

        public string Version =>
            "latest";

        private const string DownloadUrl =
            "https://raw.githubusercontent.com/wp-cli/builds/gh-pages/phar/wp-cli.phar";

        public async Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            string wpCliRoot =
                Path.Combine(
                    installationPath,
                    "tools",
                    "wp-cli");

            string wpCliPath =
                Path.Combine(
                    wpCliRoot,
                    "wp-cli.phar");

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Preparing WP-CLI...",
                    Percentage = 0
                });

            Directory.CreateDirectory(
                wpCliRoot);

            if (File.Exists(wpCliPath))
            {
                File.Delete(
                    wpCliPath);
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Downloading WP-CLI...",
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

            using (
                Stream input =
                    await response.Content.ReadAsStreamAsync(
                        cancellationToken))
            using (
                FileStream output =
                    new(
                        wpCliPath,
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
                            80;

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
                    Message =
                        "Verifying WP-CLI...",
                    Percentage = 95
                });

            if (!File.Exists(wpCliPath))
            {
                throw new InvalidOperationException(
                    $"WP-CLI installation failed. " +
                    $"The expected file was not found: {wpCliPath}");
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "WP-CLI installed.",
                    Percentage = 100
                });
        }
    }
}