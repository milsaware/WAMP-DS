namespace WAMP_DS.Models
{
    public class MagentoInstallSettings
    {
        public required string AdminUsername { get; set; }
        public required string AdminPassword { get; set; }
        public required string AdminEmail { get; set; }
        public required string AdminUrl { get; set; }
        public required string AdminFirstName { get; set; }
        public required string AdminLastName { get; set; }
        public string Language { get; set; } = "en_GB";
        public string Timezone { get; set; } = "Europe/London";
        public string Currency { get; set; } = "GBP";
    }
}