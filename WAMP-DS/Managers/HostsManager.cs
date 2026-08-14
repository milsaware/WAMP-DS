using System.IO;

namespace WAMP_DS.Managers
{
    public class HostsManager
    {
        private readonly string hostsPath =
            @"C:\Windows\System32\drivers\etc\hosts";

        public void AddHosts(IEnumerable<string> hostnames)
        {
            if (!File.Exists(hostsPath))
                throw new FileNotFoundException(
                    "Windows hosts file was not found.",
                    hostsPath
                );


            List<string> lines =
                File.ReadAllLines(hostsPath)
                    .ToList();


            bool changed = false;


            foreach (string hostname in hostnames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(hostname))
                    continue;


                bool exists =
                    lines.Any(line =>
                    {
                        string trimmed = line.Trim();


                        if (trimmed.StartsWith("#"))
                            return false;


                        string[] parts =
                            trimmed.Split(
                                new[] { ' ', '\t' },
                                StringSplitOptions.RemoveEmptyEntries
                            );


                        return parts.Length >= 2 &&
                               parts.Skip(1)
                                    .Any(x =>
                                        x.Equals(
                                            hostname,
                                            StringComparison.OrdinalIgnoreCase
                                        ));
                    });


                if (exists)
                    continue;


                lines.Add(
                    $"127.0.0.1    {hostname}"
                );


                changed = true;
            }


            if (changed)
            {
                File.WriteAllLines(
                    hostsPath,
                    lines
                );
            }
        }
    }
}