using System.IO;
using System.IO.Compression;
using System.Net.Http;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public class OpenSearchInstaller : IInstaller
    {
        public string Name =>
            "OpenSearch";

        public string Version =>
            "3.8.0";

        private const string DownloadUrl =
            "https://artifacts.opensearch.org/releases/bundle/opensearch/3.8.0/opensearch-3.8.0-windows-x64.zip";

        public async Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            string openSearchRoot =
                Path.Combine(
                    installationPath,
                    "runtimes",
                    "opensearch",
                    Version);

            string tempRoot =
                Path.Combine(
                    Path.GetTempPath(),
                    "WAMP-DS",
                    "opensearch");

            string zipPath =
                Path.Combine(
                    tempRoot,
                    "opensearch.zip");

            string extractRoot =
                Path.Combine(
                    tempRoot,
                    "extracted");

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"Preparing OpenSearch {Version}...",
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
                        $"Downloading OpenSearch {Version}...",
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
                        $"Extracting OpenSearch {Version}...",
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

            string extractedOpenSearchRoot =
                Directory.GetDirectories(
                    extractRoot)
                    .FirstOrDefault()
                ?? throw new InvalidOperationException(
                    "The OpenSearch archive does not contain an expected OpenSearch directory.");

            if (Directory.Exists(openSearchRoot))
            {
                Directory.Delete(
                    openSearchRoot,
                    true);
            }

            Directory.CreateDirectory(
                openSearchRoot);

            foreach (string directory in
                Directory.GetDirectories(
                    extractedOpenSearchRoot))
            {
                string destination =
                    Path.Combine(
                        openSearchRoot,
                        Path.GetFileName(directory));

                Directory.Move(
                    directory,
                    destination);
            }

            foreach (string file in
                Directory.GetFiles(
                    extractedOpenSearchRoot))
            {
                string destination =
                    Path.Combine(
                        openSearchRoot,
                        Path.GetFileName(file));

                File.Move(
                    file,
                    destination);
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Configuring OpenSearch...",
                    Percentage = 90
                }
            );

            ConfigureOpenSearch(installationPath);

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"Verifying OpenSearch {Version}...",
                    Percentage = 95
                });

            string openSearchBatchPath =
                Path.Combine(
                    openSearchRoot,
                    "bin",
                    "opensearch.bat");

            if (!File.Exists(
                    openSearchBatchPath))
            {
                throw new InvalidOperationException(
                    $"OpenSearch installation failed. " +
                    $"The expected file was not found: {openSearchBatchPath}");
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"OpenSearch {Version} installed.",
                    Percentage = 100
                });

            try
            {
                File.Delete(
                    zipPath);

                if (Directory.Exists(
                    extractRoot))
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

        private void ConfigureOpenSearch(
    string installationPath)
        {
            string openSearchRoot =
                Path.Combine(
                    installationPath,
                    "runtimes",
                    "opensearch",
                    Version);

            string dataRoot =
                Path.Combine(
                    openSearchRoot,
                    "data");

            string logsRoot =
                Path.Combine(
                    openSearchRoot,
                    "logs");

            string templatePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    "OpenSearch",
                    "opensearch.yml");

            string configPath =
                Path.Combine(
                    openSearchRoot,
                    "config",
                    "opensearch.yml");

            Directory.CreateDirectory(
                dataRoot);

            Directory.CreateDirectory(
                logsRoot);

            string config =
                File.ReadAllText(
                    templatePath);

            config = config.Replace(
                "{{OPENSEARCH_DATA}}",
                dataRoot.Replace(
                    "\\",
                    "/"));

            config = config.Replace(
                "{{OPENSEARCH_LOGS}}",
                logsRoot.Replace(
                    "\\",
                    "/"));

            File.WriteAllText(
                configPath,
                config);
        }
    }
}