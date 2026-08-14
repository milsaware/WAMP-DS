using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public class PhpMyAdminInstaller : IInstaller
    {
        public string Name =>
            "phpMyAdmin";

        public string Version =>
            "5.2.3";

        private const string DownloadUrl =
            "https://files.phpmyadmin.net/phpMyAdmin/5.2.3/phpMyAdmin-5.2.3-all-languages.zip";

        public async Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            string phpMyAdminRoot =
                Path.Combine(
                    installationPath,
                    "runtimes",
                    "phpmyadmin",
                    Version);

            string tempRoot =
                Path.Combine(
                    Path.GetTempPath(),
                    "WAMP-DS",
                    "phpmyadmin");

            string zipPath =
                Path.Combine(
                    tempRoot,
                    "phpmyadmin.zip");

            string extractRoot =
                Path.Combine(
                    tempRoot,
                    "extracted");

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"Preparing phpMyAdmin {Version}...",
                    Percentage = 0
                });

            Directory.CreateDirectory(
                tempRoot);

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            if (Directory.Exists(extractRoot))
            {
                Directory.Delete(
                    extractRoot,
                    true);
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"Downloading phpMyAdmin {Version}...",
                    Percentage = 10
                });

            using HttpClient client =
                new();

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
                FileStream zipOutput =
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
                    await zipOutput.WriteAsync(
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
                                Message =
                                    "",
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
                        $"Extracting phpMyAdmin {Version}...",
                    Percentage = 80
                });

            Directory.CreateDirectory(
                extractRoot);

            await Task.Run(
                () =>
                    ZipFile.ExtractToDirectory(
                        zipPath,
                        extractRoot),
                cancellationToken);

            if (Directory.Exists(phpMyAdminRoot))
            {
                Directory.Delete(
                    phpMyAdminRoot,
                    true);
            }

            Directory.CreateDirectory(
                phpMyAdminRoot);

            string extractedRoot =
                Directory.GetDirectories(
                    extractRoot)
                    .FirstOrDefault()
                ?? throw new InvalidOperationException(
                    "The phpMyAdmin archive does not contain an expected directory.");

            foreach (string directory in
                Directory.GetDirectories(extractedRoot))
            {
                string destination =
                    Path.Combine(
                        phpMyAdminRoot,
                        Path.GetFileName(directory));

                Directory.Move(
                    directory,
                    destination);
            }

            foreach (string file in
                Directory.GetFiles(extractedRoot))
            {
                string destination =
                    Path.Combine(
                        phpMyAdminRoot,
                        Path.GetFileName(file));

                File.Move(
                    file,
                    destination);
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"Verifying phpMyAdmin {Version}...",
                    Percentage = 95
                });

            string indexPath =
                Path.Combine(
                    phpMyAdminRoot,
                    "index.php");

            if (!File.Exists(indexPath))
            {
                throw new InvalidOperationException(
                    $"phpMyAdmin installation failed. " +
                    $"The expected file was not found: {indexPath}");
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Configuring phpMyAdmin...",
                    Percentage = 98
                }
            );

            ConfigurePhpMyAdmin(
                installationPath,
                phpMyAdminRoot);

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"phpMyAdmin {Version} installed.",
                    Percentage = 100
                });

            try
            {
                File.Delete(zipPath);

                if (Directory.Exists(extractRoot))
                {
                    Directory.Delete(
                        extractRoot,
                        true);
                }
            }
            catch
            {
                // Temporary files can be cleaned up later.
            }
        }

        private void ConfigurePhpMyAdmin(
            string installationPath,
            string phpMyAdminRoot)
        {
            string templatePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    "phpMyAdmin",
                    "config.inc.php");

            string configPath =
                Path.Combine(
                    phpMyAdminRoot,
                    "config.inc.php");

            string config =
                File.ReadAllText(
                    templatePath);

            byte[] secretBytes =
                RandomNumberGenerator.GetBytes(32);

            string blowfishSecret =
                Convert.ToBase64String(
                    secretBytes);

            config = config.Replace(
                "{{BLOWFISH_SECRET}}",
                blowfishSecret);

            File.WriteAllText(
                configPath,
                config);
        }
    }
}