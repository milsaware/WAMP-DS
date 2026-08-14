namespace WAMP_DS.Models
{
    public class WordPressInstallSettings
    {
        public string SiteTitle { get; set; } = string.Empty;

        public string AdminUsername { get; set; } = string.Empty;

        public string AdminPassword { get; set; } = string.Empty;

        public string AdminEmail { get; set; } = string.Empty;

        public bool DiscourageSearchEngines { get; set; }
    }
}