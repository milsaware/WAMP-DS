using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Installers
{
    public interface IInstaller
    {
        string Name { get; }

        Task InstallAsync(
            string installationPath,
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }
}