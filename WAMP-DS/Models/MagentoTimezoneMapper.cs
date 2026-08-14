using System.Collections.Generic;

namespace WAMP_DS.Managers
{
    public class MagentoTimezoneMapper
    {
        private readonly Dictionary<string, string> mappings = new()
        {
            { "GMT Standard Time", "Europe/London" },
            { "W. Europe Standard Time", "Europe/Berlin" },
            { "Romance Standard Time", "Europe/Paris" },
            { "Central Europe Standard Time", "Europe/Budapest" },
            { "Eastern Standard Time", "America/New_York" },
            { "Pacific Standard Time", "America/Los_Angeles" },
            { "China Standard Time", "Asia/Shanghai" },
            { "Tokyo Standard Time", "Asia/Tokyo" },
            { "India Standard Time", "Asia/Kolkata" },
            { "UTC", "UTC" }
        };

        public string Convert(string windowsTimezone)
        {
            if (string.IsNullOrEmpty(windowsTimezone))
                return "UTC";

            if (mappings.TryGetValue(windowsTimezone, out var magentoTimezone))
                return magentoTimezone;

            return windowsTimezone;
        }
    }
}