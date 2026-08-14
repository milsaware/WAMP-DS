using System.IO;
using Microsoft.Win32;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public class WindowsRegistrationInstaller : IInstaller
    {
        public string Name =>
            "Windows Application Registration";

        public Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string uninstallPath =
                Path.Combine(
                    installationPath,
                    "Uninstaller",
                    "WAMP-DS.Uninstaller.exe");

            using RegistryKey key =
                Registry.CurrentUser.CreateSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Uninstall\WAMP-DS");

            key.SetValue(
                "DisplayName",
                "WAMP-DS");

            key.SetValue(
                "DisplayVersion",
                "1.0.0");

            key.SetValue(
                "Publisher",
                "Milsaware");

            key.SetValue(
                "InstallLocation",
                installationPath);

            key.SetValue(
                "UninstallString",
                $"\"{uninstallPath}\"");

            key.SetValue(
                "DisplayIcon",
                Path.Combine(
                    installationPath,
                    "WAMP-DS.exe"));

            progress?.Report(
                new InstallationProgress
                {
                    Message =
                        "WAMP-DS registered with Windows.",
                    Percentage = 100
                });

            return Task.CompletedTask;
        }
    }
}