using System.IO;

namespace WAMP_DS.Core
{
    public class InstallationValidator
    {
        private readonly InstallationPaths paths;

        public InstallationValidator(
            InstallationPaths paths)
        {
            this.paths = paths;
        }


        public bool IsValid()
        {
            return
                Directory.Exists(paths.ApachePath) &&
                Directory.Exists(paths.PhpPath) &&
                Directory.Exists(paths.MySqlPath);
        }
    }
}