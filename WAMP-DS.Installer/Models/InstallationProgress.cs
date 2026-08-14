namespace WAMP_DS.Installer.Models
{
    public class InstallationProgress
    {
        public string? Message { get; set; }

        public double Percentage { get; set; }

        public bool IsDetail { get; set; }
    }
}