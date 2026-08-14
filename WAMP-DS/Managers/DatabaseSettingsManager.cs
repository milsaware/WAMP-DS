using System.IO;
using System.Text.Json;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class DatabaseSettingsManager
    {
        private readonly string settingsFile;


        public DatabaseSettingsManager()
        {
            string settingsDirectory =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "settings"
                );


            Directory.CreateDirectory(
                settingsDirectory
            );


            settingsFile =
                Path.Combine(
                    settingsDirectory,
                    "mysql.json"
                );
        }


        public DatabaseCredentials Load()
        {
            if (!File.Exists(settingsFile))
            {
                return new DatabaseCredentials();
            }


            string json =
                File.ReadAllText(
                    settingsFile
                );


            return JsonSerializer.Deserialize<DatabaseCredentials>(
                json
            )
            ?? new DatabaseCredentials();
        }


        public void Save(
            DatabaseCredentials credentials)
        {
            string json =
                JsonSerializer.Serialize(
                    credentials,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );


            File.WriteAllText(
                settingsFile,
                json
            );
        }
    }
}