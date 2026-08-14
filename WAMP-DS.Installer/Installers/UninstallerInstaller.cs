using System.IO;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public class UninstallerInstaller : IInstaller
    {
        public string Name =>
            "WAMP-DS Uninstaller";

        public Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            string sourcePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Packages",
                    "Uninstaller");

            string destinationPath =
                Path.Combine(
                    installationPath,
                    "Uninstaller");

            if (!Directory.Exists(sourcePath))
            {
                throw new DirectoryNotFoundException(
                    $"WAMP-DS Uninstaller package was not found: {sourcePath}");
            }

            cancellationToken.ThrowIfCancellationRequested();

            Directory.CreateDirectory(
                destinationPath);

            CopyDirectory(
                sourcePath,
                destinationPath,
                cancellationToken);

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "WAMP-DS Uninstaller installed.",
                    Percentage = 100
                });

            return Task.CompletedTask;
        }


        private static void CopyDirectory(
            string sourceDirectory,
            string destinationDirectory,
            CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(
                destinationDirectory);

            foreach (string file in
                Directory.GetFiles(sourceDirectory))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string destinationFile =
                    Path.Combine(
                        destinationDirectory,
                        Path.GetFileName(file));

                File.Copy(
                    file,
                    destinationFile,
                    true);
            }

            foreach (string directory in
                Directory.GetDirectories(sourceDirectory))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string destinationSubDirectory =
                    Path.Combine(
                        destinationDirectory,
                        Path.GetFileName(directory));

                CopyDirectory(
                    directory,
                    destinationSubDirectory,
                    cancellationToken);
            }
        }
    }
}