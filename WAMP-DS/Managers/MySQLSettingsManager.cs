using System.IO;
using System.Text.Json;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class MySQLSettingsManager
    {
        private readonly string settingsPath;

        public MySQLSettings Settings { get; private set; }


        public MySQLSettingsManager()
        {
            settingsPath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "settings",
                    "mysql.json"
                );


            Settings =
                Load();
        }


        public MySQLSettings Load()
        {
            if (!File.Exists(settingsPath))
            {
                return new MySQLSettings();
            }


            string json =
                File.ReadAllText(
                    settingsPath
                );


            return JsonSerializer.Deserialize<MySQLSettings>(
                json
            )
            ?? new MySQLSettings();
        }


        public void Save()
        {
            string directory =
                Path.GetDirectoryName(
                    settingsPath
                )!;


            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }


            string json =
                JsonSerializer.Serialize(
                    Settings,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );


            File.WriteAllText(
                settingsPath,
                json
            );
        }
    }
}