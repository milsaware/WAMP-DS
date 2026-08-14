using System.Diagnostics;
using System.IO;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class DatabaseManager
    {
        private readonly string mysqlExecutable;
        private readonly MySQLSettings mysqlSettings;

        public DatabaseManager(MySQLSettingsManager mysqlSettingsManager)
        {
            mysqlSettings =
                mysqlSettingsManager.Settings;

            mysqlExecutable =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "runtimes",
                    "mysql",
                    "8.4.11",
                    "bin",
                    "mysql.exe"
                );
        }


        public async Task CreateDatabase(
            string databaseName)
        {
            if (!File.Exists(mysqlExecutable))
            {
                throw new FileNotFoundException(
                    "MySQL executable not found.",
                    mysqlExecutable
                );
            }


            ProcessStartInfo startInfo =
                new()
                {
                    FileName = mysqlExecutable,

                    Arguments =
    string.IsNullOrWhiteSpace(mysqlSettings.Password)
    ?
    $"-u {mysqlSettings.Username} -e \"CREATE DATABASE `{databaseName}`;\""
    :
    $"-u {mysqlSettings.Username} -p{mysqlSettings.Password} -e \"CREATE DATABASE `{databaseName}`;\"",

                    UseShellExecute = false,

                    CreateNoWindow = true,

                    RedirectStandardOutput = true,

                    RedirectStandardError = true
                };


            using Process process =
                new()
                {
                    StartInfo = startInfo
                };


            process.Start();


            string error =
                await process.StandardError.ReadToEndAsync();


            await process.WaitForExitAsync();


            if (process.ExitCode != 0)
            {
                throw new Exception(
                    $"Database creation failed: {error}"
                );
            }
        }
    }
}