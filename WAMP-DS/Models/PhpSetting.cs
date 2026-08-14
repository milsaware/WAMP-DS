namespace WAMP_DS.Models
{
    public class PhpSetting
    {
        public string Section { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public bool IsEnabled { get; set; }

        public bool IsCommented { get; set; }

        public PhpSettingType Type { get; set; }

        public string OriginalLine { get; set; } = string.Empty;
    }
}