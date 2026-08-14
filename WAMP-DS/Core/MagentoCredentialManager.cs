using System.IO;
using System.Text.Json;
using WAMP_DS.Models;

namespace WAMP_DS.Core
{
    public class MagentoCredentialManager
    {
        private readonly string settingsPath;

        public MagentoCredentialManager()
        {
            settingsPath = Path.Combine(
                AppContext.BaseDirectory,
                "settings",
                "magento.json"
            );
        }

        public MagentoSettings Load()
        {
            if (!File.Exists(settingsPath))
            {
                return new MagentoSettings();
            }

            string json = File.ReadAllText(settingsPath);

            return JsonSerializer.Deserialize<MagentoSettings>(
                json
            )
            ?? new MagentoSettings();
        }

        public void Save(
            MagentoSettings settings
        )
        {
            string? directory = Path.GetDirectoryName(settingsPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            string json = JsonSerializer.Serialize(
                settings,
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