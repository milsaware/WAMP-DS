using System.IO;
using System.Runtime.InteropServices;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public class ShortcutInstaller : IInstaller
    {
        public string Name =>
            "WAMP-DS shortcuts";

        public Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            string exePath =
                Path.Combine(
                    installationPath,
                    "WAMP-DS.exe");

            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException(
                    "WAMP-DS executable not found.",
                    exePath);
            }

            string desktopPath =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.DesktopDirectory);

            string startMenuPath =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.StartMenu),
                    "Programs",
                    "WAMP-DS");

            Directory.CreateDirectory(
                startMenuPath);

            CreateShortcut(
                Path.Combine(
                    desktopPath,
                    "WAMP-DS.lnk"),
                exePath);

            CreateShortcut(
                Path.Combine(
                    startMenuPath,
                    "WAMP-DS.lnk"),
                exePath);

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "Desktop and Start Menu shortcuts created.",
                    Percentage = 100
                });

            return Task.CompletedTask;
        }


        private static void CreateShortcut(
            string shortcutPath,
            string targetPath)
        {
            Type? shellType =
                Type.GetTypeFromProgID(
                    "WScript.Shell");

            if (shellType == null)
            {
                throw new InvalidOperationException(
                    "Windows Script Host is unavailable.");
            }

            dynamic shell =
                Activator.CreateInstance(
                    shellType)!;

            dynamic shortcut =
                shell.CreateShortcut(
                    shortcutPath);

            shortcut.TargetPath =
                targetPath;

            shortcut.WorkingDirectory =
                Path.GetDirectoryName(
                    targetPath);

            shortcut.Description =
                "WAMP-DS - PHP Development Environment";

            shortcut.IconLocation =
                $"{targetPath},0";

            shortcut.Save();

            Marshal.FinalReleaseComObject(
                shortcut);

            Marshal.FinalReleaseComObject(
                shell);
        }
    }
}