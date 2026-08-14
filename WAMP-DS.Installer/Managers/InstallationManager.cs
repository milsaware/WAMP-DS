using WAMP_DS.Installer.Installers;
using WAMP_DS.Installer.Models;

namespace WAMP_DS.Installer.Managers
{
    public class InstallationManager
    {
        private readonly InstallationOptions options;

        private readonly List<IInstaller> installers;


        public InstallationManager(
            InstallationOptions options)
        {
            this.options = options;

            installers =
            [
                new WampDsInstaller(),
                new ApacheInstaller(),
                new PhpInstaller(),
                new MySqlInstaller(),
                new PhpMyAdminInstaller(),
                new OpenSearchInstaller(),
                new ComposerInstaller(),
                new WpCliInstaller(),
                new ShortcutInstaller(),
                new UninstallerInstaller(),
                new WindowsRegistrationInstaller()
            ];
        }


        public async Task InstallAsync(
            IProgress<InstallationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            foreach (IInstaller installer in installers)
            {
                progress?.Report(
                    new InstallationProgress
                    {
                        Message =
                            $"Installing {installer.Name}...",
                        Percentage = 0
                    });


                await installer.InstallAsync(
                    options.InstallationPath,
                    progress,
                    cancellationToken);


                progress?.Report(
                    new InstallationProgress
                    {
                        Message =
                            $"{installer.Name} installed.",
                        Percentage = 100
                    });
            }
        }
    }
}