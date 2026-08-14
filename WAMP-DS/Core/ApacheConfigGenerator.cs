using System.IO;

namespace WAMP_DS.Core
{
    public class ApacheConfigGenerator
    {
        private readonly InstallationPaths paths;

        public ApacheConfigGenerator(
            InstallationPaths paths)
        {
            this.paths = paths;
        }


        public void Generate(
            string documentRoot)
        {
            string templatePath =
                Path.Combine(
                    paths.RootPath,
                    "templates",
                    "httpd.conf.template"
                );


            string outputPath =
                Path.Combine(
                    paths.ApachePath,
                    "conf",
                    "httpd.conf"
                );


            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(
                    "Apache configuration template was not found.",
                    templatePath
                );
            }


            string config =
                File.ReadAllText(
                    templatePath
                );


            config =
                config.Replace(
                    "{{SRVROOT}}",
                    paths.ApachePath
                );


            config =
                config.Replace(
                    "{{PHPROOT}}",
                    paths.PhpPath
                );


            config =
                config.Replace(
                    "{{PHPMYADMINROOT}}",
                    paths.PhpMyAdminPath
                );


            config =
                config.Replace(
                    "{{DOCUMENTROOT}}",
                    documentRoot
                );


            File.WriteAllText(
                outputPath,
                config
            );
        }
    }
}