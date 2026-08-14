using System.IO;

namespace WAMP_DS.Core
{
    public class InstallationPaths
    {
        public string RootPath { get; }

        public InstallationPaths()
        {
            RootPath = AppContext.BaseDirectory;
        }

        public string RuntimePath =>
            Path.Combine(
                RootPath,
                "runtimes");

        public string ApachePath =>
            Path.Combine(
                RuntimePath,
                "apache",
                "2.4.68");

        public string PhpPath =>
            Path.Combine(
                RuntimePath,
                "php",
                "8.5.8");

        public string MySqlPath =>
            Path.Combine(
                RuntimePath,
                "mysql",
                "8.4.11");

        public string ToolsPath =>
            Path.Combine(
                RootPath,
                "tools");

        public string PhpMyAdminPath =>
            Path.Combine(
                ToolsPath,
                "phpmyadmin",
                "5.2.3");

        public string OpenSearchPath =>
            Path.Combine(
                RuntimePath,
                "opensearch",
                "3.8.0");
    }
}