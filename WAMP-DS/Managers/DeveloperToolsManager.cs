using System.Text;
using System.Text.RegularExpressions;

namespace WAMP_DS.Managers
{
    public class DeveloperToolsManager
    {
        public string GenerateEnvironmentReport()
        {
            StringBuilder report = new();

            report.AppendLine(
                "=== WAMP-DS Developer Report ==="
            );

            report.AppendLine();

            report.AppendLine(
                $"Generated: {DateTime.Now}"
            );

            report.AppendLine();

            report.AppendLine(
                "Environment:"
            );

            report.AppendLine(
                $"OS: {Environment.OSVersion}"
            );

            report.AppendLine(
                $".NET: {Environment.Version}"
            );

            report.AppendLine();

            return report.ToString();
        }
    }
}