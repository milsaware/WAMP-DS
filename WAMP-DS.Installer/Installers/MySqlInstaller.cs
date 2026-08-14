using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public class MySqlInstaller : IInstaller
    {
        public string Name =>
            "MySQL";

        public string Version =>
            "8.4.11";

        private const string DownloadUrl =
            "https://cdn.mysql.com/Downloads/MySQL-8.4/mysql-8.4.11-winx64.zip";


        public async Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            string mysqlRoot =
                Path.Combine(
                    installationPath,
                    "runtimes",
                    "mysql",
                    Version);

            string tempRoot =
                Path.Combine(
                    Path.GetTempPath(),
                    "WAMP-DS",
                    "mysql");

            string zipPath =
                Path.Combine(
                    tempRoot,
                    "mysql.zip");

            string extractRoot =
                Path.Combine(
                    tempRoot,
                    "extracted");


            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"Preparing MySQL {Version}...",
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
                        $"Downloading MySQL {Version}...",
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
                        $"Extracting MySQL {Version}...",
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


            if (Directory.Exists(mysqlRoot))
            {
                Directory.Delete(
                    mysqlRoot,
                    true);
            }


            Directory.CreateDirectory(
                mysqlRoot);


            string extractedMysqlPath =
                Directory.GetDirectories(
                    extractRoot)
                    .FirstOrDefault()
                ?? throw new InvalidOperationException(
                    "The MySQL archive does not contain an expected MySQL directory.");


            foreach (
                string directory in
                Directory.GetDirectories(
                    extractedMysqlPath))
            {
                string destination =
                    Path.Combine(
                        mysqlRoot,
                        Path.GetFileName(directory));


                Directory.Move(
                    directory,
                    destination);
            }


            foreach (
                string file in
                Directory.GetFiles(
                    extractedMysqlPath))
            {
                string destination =
                    Path.Combine(
                        mysqlRoot,
                        Path.GetFileName(file));


                File.Move(
                    file,
                    destination);
            }


            string mysqldPath =
                Path.Combine(
                    mysqlRoot,
                    "bin",
                    "mysqld.exe");


            if (!File.Exists(mysqldPath))
            {
                throw new InvalidOperationException(
                    $"MySQL installation failed. " +
                    $"The expected file was not found: {mysqldPath}");
            }


            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Creating MySQL directories...",
                    Percentage = 85
                });


            string dataRoot =
                Path.Combine(
                    mysqlRoot,
                    "data");


            string tempRootMysql =
                Path.Combine(
                    mysqlRoot,
                    "tmp");


            string logsRoot =
                Path.Combine(
                    mysqlRoot,
                    "logs");


            Directory.CreateDirectory(
                dataRoot);


            Directory.CreateDirectory(
                tempRootMysql);


            Directory.CreateDirectory(
                logsRoot);


            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Configuring MySQL...",
                    Percentage = 90
                });


            string configPath =
                Path.Combine(
                    mysqlRoot,
                    "my.ini");


            string config =
                $"""
                [mysqld]
                basedir="{mysqlRoot.Replace("\\", "/")}"
                datadir="{dataRoot.Replace("\\", "/")}"
                port=3306
                bind-address=127.0.0.1
                max_connections=151
                character-set-server=utf8mb4
                collation-server=utf8mb4_0900_ai_ci
                sql-mode=""
                log-error="{Path.Combine(logsRoot, "mysql_error.log").Replace("\\", "/")}"
                tmpdir="{tempRootMysql.Replace("\\", "/")}"

                [client]
                port=3306
                default-character-set=utf8mb4
                """;


            File.WriteAllText(
                configPath,
                config);


            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Initialising MySQL data directory...",
                    Percentage = 95
                });


            ProcessStartInfo initialiseInfo =
                new()
                {
                    FileName =
                        mysqldPath,

                    Arguments =
                        $"--defaults-file=\"{configPath}\" " +
                        "--initialize-insecure",

                    WorkingDirectory =
                        mysqlRoot,

                    UseShellExecute =
                        false,

                    CreateNoWindow =
                        true,

                    RedirectStandardOutput =
                        true,

                    RedirectStandardError =
                        true
                };


            using Process process =
                new()
                {
                    StartInfo =
                        initialiseInfo
                };


            process.Start();


            string output =
                await process.StandardOutput.ReadToEndAsync(
                    cancellationToken);


            string error =
                await process.StandardError.ReadToEndAsync(
                    cancellationToken);


            await process.WaitForExitAsync(
                cancellationToken);


            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    "MySQL initialisation failed." +
                    Environment.NewLine +
                    error +
                    Environment.NewLine +
                    output);
            }


            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"Verifying MySQL {Version}...",
                    Percentage = 98
                });


            if (!File.Exists(mysqldPath))
            {
                throw new InvalidOperationException(
                    $"MySQL installation failed. " +
                    $"The expected executable was not found: {mysqldPath}");
            }


            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        $"MySQL {Version} installed.",
                    Percentage = 100
                });


            try
            {
                File.Delete(
                    zipPath);


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
    }
}