namespace WAMP_DS.Models
{
    public class MagentoInstallConfiguration
    {
        // Database
        public string DatabaseHost { get; set; } = "localhost";

        public string DatabaseName { get; set; } = "";

        public string DatabaseUsername { get; set; } = "";

        public string DatabasePassword { get; set; } = "";

        // Magento URLs
        public string BaseUrl { get; set; } = "";

        public string AdminUri { get; set; } = "admin";

        // Administrator account
        public string AdminUsername { get; set; } = "";

        public string AdminPassword { get; set; } = "";

        public string AdminEmail { get; set; } = "";

        public string AdminFirstName { get; set; } = "Admin";

        public string AdminLastName { get; set; } = "User";
    }
}