using System.IO;
using System.IO.Compression;
using System.Net.Http;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public class ApacheInstaller : IInstaller
    {
        public string Name =>
            "Apache HTTP Server";

        public string Version =>
            "2.4.68";

        private const string DownloadUrl =
    "https://www.apachelounge.com/download/VS18/binaries/httpd-2.4.68-260617-Win64-VS18.zip";

        public async Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            string apacheRoot =
                Path.Combine(
                    installationPath,
                    "runtimes",
                    "apache",
                    Version);

            string tempDirectory =
                Path.Combine(
                    Path.GetTempPath(),
                    "WAMP-DS",
                    "apache");

            string zipPath =
                Path.Combine(
                    tempDirectory,
                    "apache.zip");


            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"Preparing Apache {Version}...",
                    Percentage = 0
                });


            Directory.CreateDirectory(
                apacheRoot);

            Directory.CreateDirectory(
                tempDirectory);


            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }


            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"Downloading Apache {Version}...",
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
                    Message =
                        $"Extracting Apache {Version}...",
                    Percentage = 80
                });


            if (Directory.Exists(apacheRoot))
            {
                Directory.Delete(
                    apacheRoot,
                    true);
            }

            Directory.CreateDirectory(
                apacheRoot);


            string extractDirectory =
    Path.Combine(
        tempDirectory,
        "extracted");

            if (Directory.Exists(extractDirectory))
            {
                Directory.Delete(
                    extractDirectory,
                    true);
            }

            await Task.Run(
                () =>
                    ZipFile.ExtractToDirectory(
                        zipPath,
                        extractDirectory),
                cancellationToken);


            string extractedApachePath =
                Path.Combine(
                    extractDirectory,
                    "Apache24");


            if (!Directory.Exists(extractedApachePath))
            {
                throw new InvalidOperationException(
                    "The Apache archive does not contain the expected Apache24 directory.");
            }


            foreach (string directory in
                Directory.GetDirectories(extractedApachePath))
            {
                string destination =
                    Path.Combine(
                        apacheRoot,
                        Path.GetFileName(directory));

                Directory.Move(
                    directory,
                    destination);
            }


            foreach (string file in
                Directory.GetFiles(extractedApachePath))
            {
                string destination =
                    Path.Combine(
                        apacheRoot,
                        Path.GetFileName(file));

                File.Move(
                    file,
                    destination);
            }


            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"Verifying Apache {Version}...",
                    Percentage = 95
                });


            string httpdPath =
                Path.Combine(
                    apacheRoot,
                    "bin",
                    "httpd.exe");


            if (!File.Exists(httpdPath))
            {
                throw new InvalidOperationException(
                    $"Apache installation failed. " +
                    $"The expected file was not found: {httpdPath}");
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message = "Configuring Apache...",
                    Percentage = 95
                });

            ConfigureApache(installationPath);

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"Apache {Version} installed.",
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

        private void ConfigureApache(
            string installationPath)
        {
            string installRoot = installationPath;

            string apacheRoot =
                Path.Combine(
                    installRoot,
                    "runtimes",
                    "apache",
                    "2.4.68");

            string phpRoot =
                Path.Combine(
                    installRoot,
                    "runtimes",
                    "php",
                    "8.5.8");

            string phpMyAdminRoot =
                Path.Combine(
                    installRoot,
                    "runtimes",
                    "phpmyadmin",
                    "5.2.3");

            string templatePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    "Apache",
                    "httpd.conf");

            string configPath =
                Path.Combine(
                    apacheRoot,
                    "conf",
                    "httpd.conf");

            string sslTemplatePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    "Apache",
                    "httpd-ssl.conf");

            string sslConfigPath =
                Path.Combine(
                    apacheRoot,
                    "conf",
                    "extra",
                    "httpd-ssl.conf");

            string config =
                File.ReadAllText(templatePath);

            config = config.Replace(
                "{{SRVROOT}}",
                apacheRoot);

            config = config.Replace(
                "{{DOCROOT}}",
                installRoot);

            config = config.Replace(
                "{{PHPROOT}}",
                phpRoot);

            config = config.Replace(
                "{{PHPMYADMINROOT}}",
                phpMyAdminRoot);

            File.WriteAllText(
                configPath,
                config);

            string sslConfig = File.ReadAllText(
                sslTemplatePath
            );

            File.WriteAllText(
                sslConfigPath,
                sslConfig
            );
        }
    }
}