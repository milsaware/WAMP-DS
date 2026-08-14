using System.IO;
using System.Net.Http;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public class ComposerInstaller : IInstaller
    {
        public string Name =>
            "Composer";

        public string Version =>
            "latest-stable";

        private const string DownloadUrl =
            "https://getcomposer.org/download/latest-stable/composer.phar";

        public async Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            string composerRoot =
                Path.Combine(
                    installationPath,
                    "tools",
                    "composer");

            string composerPath =
                Path.Combine(
                    composerRoot,
                    "composer.phar");

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Preparing Composer...",
                    Percentage = 0
                });

            Directory.CreateDirectory(
                composerRoot);

            if (File.Exists(composerPath))
            {
                File.Delete(
                    composerPath);
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Downloading Composer...",
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
                        composerPath,
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
                        "Verifying Composer...",
                    Percentage = 95
                });

            if (!File.Exists(composerPath))
            {
                throw new InvalidOperationException(
                    $"Composer installation failed. " +
                    $"The expected file was not found: {composerPath}");
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Composer installed.",
                    Percentage = 100
                });
        }
    }
}