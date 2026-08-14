using WAMP_DS.Core;

namespace WAMP_DS.Managers
{
    public class MagentoRequirementManager
    {
        private readonly PhpSettingsManager phpSettings;

        private readonly string[] requiredExtensions =
        {
            "xsl",
            "intl",
            "soap",
            "sockets",
            "sodium",
            "zip"
        };

        public MagentoRequirementManager(
            PhpSettingsManager phpSettings)
        {
            this.phpSettings = phpSettings;
        }

        public void Prepare()
        {
            foreach (string extension in requiredExtensions)
            {
                try
                {
                    phpSettings.SetEnabled(
                        "PHP",
                        extension,
                        true
                    );
                }
                catch
                {
                    // Extension not found.
                    // Leave it alone.
                }
            }

            phpSettings.Save();
        }
    }
}