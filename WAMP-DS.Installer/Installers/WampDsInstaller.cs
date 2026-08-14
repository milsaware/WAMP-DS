using System.IO;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public class WampDsInstaller : IInstaller
    {
        public string Name =>
            "WAMP-DS";

        public string Version =>
            "Current";

        public async Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            string packagePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Packages",
                    "WAMP-DS");

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Preparing WAMP-DS...",
                    Percentage = 0
                });

            if (!Directory.Exists(packagePath))
            {
                throw new DirectoryNotFoundException(
                    $"The WAMP-DS package was not found: {packagePath}");
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Installing WAMP-DS...",
                    Percentage = 10
                });

            await Task.Run(
                () =>
                    CopyDirectory(
                        packagePath,
                        installationPath,
                        cancellationToken,
                        progress),
                cancellationToken);

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Verifying WAMP-DS...",
                    Percentage = 95
                });

            string executablePath =
                Path.Combine(
                    installationPath,
                    "WAMP-DS.exe");

            if (!File.Exists(executablePath))
            {
                throw new InvalidOperationException(
                    $"WAMP-DS installation failed. " +
                    $"The expected file was not found: {executablePath}");
            }

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "WAMP-DS installed.",
                    Percentage = 100
                });
        }

        private static void CopyDirectory(
            string sourceDirectory,
            string destinationDirectory,
            CancellationToken cancellationToken,
            IProgress<InstallationProgress>? progress)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Directory.CreateDirectory(
                destinationDirectory);

            foreach (string directory in
                Directory.GetDirectories(sourceDirectory))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string destination =
                    Path.Combine(
                        destinationDirectory,
                        Path.GetFileName(directory));

                CopyDirectory(
                    directory,
                    destination,
                    cancellationToken,
                    progress);
            }

            foreach (string file in
                Directory.GetFiles(sourceDirectory))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string destination =
                    Path.Combine(
                        destinationDirectory,
                        Path.GetFileName(file));

                File.Copy(
                    file,
                    destination,
                    true);
            }
        }
    }
}