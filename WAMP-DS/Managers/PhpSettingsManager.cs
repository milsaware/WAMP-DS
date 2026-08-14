using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class PhpSettingsManager
    {
        private readonly string phpIniPath;

        private readonly List<string> lines = new();

        public PhpSettingsManager(string phpDirectory)
        {
            if (string.IsNullOrWhiteSpace(phpDirectory))
                throw new ArgumentException(
                    "PHP directory cannot be empty.",
                    nameof(phpDirectory));

            phpIniPath = Path.Combine(
                phpDirectory,
                "php.ini");

            if (!File.Exists(phpIniPath))
            {
                throw new FileNotFoundException(
                    "PHP configuration file was not found.",
                    phpIniPath);
            }

            Load();
        }

        public string PhpIniPath => phpIniPath;

        public void EnableExtension(string extensionName)
        {
            if (string.IsNullOrWhiteSpace(extensionName))
                throw new ArgumentException(
                    "Extension name cannot be empty.",
                    nameof(extensionName));


            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed =
                    lines[i].Trim();


                // Already enabled
                if (trimmed.Equals(
                    $"extension = {extensionName}",
                    StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Equals(
                    $"extension={extensionName}",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }


                // Commented extension
                if (trimmed.Equals(
                    $";extension = {extensionName}",
                    StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Equals(
                    $";extension={extensionName}",
                    StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] =
                        $"extension = {extensionName}";

                    return;
                }
            }


            // Extension line does not exist, add it
            lines.Add(
                $"extension = {extensionName}");
        }

        public IReadOnlyList<string> RawLines => lines;

        private void Load()
        {
            lines.Clear();

            lines.AddRange(
                File.ReadAllLines(phpIniPath));
        }

        public List<PhpSetting> GetSettings()
        {
            var settings = new List<PhpSetting>();

            string currentSection = string.Empty;

            foreach (string originalLine in lines)
            {
                string line = originalLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Ignore pure comments
                if (line.StartsWith(";") &&
                    !IsCommentedSetting(line))
                {
                    continue;
                }

                // Section
                Match sectionMatch =
                    Regex.Match(
                        line,
                        @"^\[(.+?)\]$");

                if (sectionMatch.Success)
                {
                    currentSection =
                        sectionMatch.Groups[1].Value.Trim();

                    continue;
                }

                bool isCommented =
                    line.StartsWith(";");

                string settingLine =
                    isCommented
                        ? line.Substring(1).Trim()
                        : line;

                Match settingMatch =
                    Regex.Match(
                        settingLine,
                        @"^([a-zA-Z0-9_.-]+)\s*=\s*(.*)$");

                if (!settingMatch.Success)
                    continue;

                string name =
                    settingMatch.Groups[1].Value.Trim();

                string value =
                    settingMatch.Groups[2].Value.Trim();

                value =
                    RemoveInlineComment(value);

                value =
                    RemoveQuotes(value);

                PhpSettingType type =
                    DetectSettingType(
                        name,
                        value);

                settings.Add(
                    new PhpSetting
                    {
                        Section = currentSection,
                        Name = name,
                        Value = value,
                        IsEnabled = !isCommented,
                        IsCommented = isCommented,
                        Type = type,
                        OriginalLine = originalLine
                    });
            }

            return settings;
        }

        private static bool IsCommentedSetting(
            string line)
        {
            string uncommented =
                line.Substring(1).Trim();

            return Regex.IsMatch(
                uncommented,
                @"^[a-zA-Z0-9_.-]+\s*=");
        }

        private static string RemoveInlineComment(
            string value)
        {
            int commentIndex =
                value.IndexOf(
                    " ;",
                    StringComparison.Ordinal);

            if (commentIndex >= 0)
            {
                return value
                    .Substring(0, commentIndex)
                    .Trim();
            }

            return value.Trim();
        }

        private static string RemoveQuotes(
            string value)
        {
            if (value.Length >= 2 &&
                ((value.StartsWith("\"") &&
                  value.EndsWith("\"")) ||
                 (value.StartsWith("'") &&
                  value.EndsWith("'"))))
            {
                return value.Substring(
                    1,
                    value.Length - 2);
            }

            return value;
        }

        private static PhpSettingType DetectSettingType(
            string name,
            string value)
        {
            if (name.Equals(
                "date.timezone",
                StringComparison.OrdinalIgnoreCase))
            {
                return PhpSettingType.Timezone;
            }

            if (name.StartsWith(
                    "extension",
                    StringComparison.OrdinalIgnoreCase))
            {
                return PhpSettingType.Extension;
            }

            if (IsBoolean(value))
            {
                return PhpSettingType.Boolean;
            }

            if (int.TryParse(
                    value,
                    out _))
            {
                return PhpSettingType.Integer;
            }

            if (name.Contains(
                    "path",
                    StringComparison.OrdinalIgnoreCase) ||
                name.Contains(
                    "dir",
                    StringComparison.OrdinalIgnoreCase))
            {
                return PhpSettingType.Path;
            }

            return PhpSettingType.String;
        }

        private static bool IsBoolean(
            string value)
        {
            return value.Equals(
                       "On",
                       StringComparison.OrdinalIgnoreCase) ||
                   value.Equals(
                       "Off",
                       StringComparison.OrdinalIgnoreCase) ||
                   value.Equals(
                       "true",
                       StringComparison.OrdinalIgnoreCase) ||
                   value.Equals(
                       "false",
                       StringComparison.OrdinalIgnoreCase) ||
                   value == "0" ||
                   value == "1";
        }

        public void SetValue(
            string section,
            string name,
            string value)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed =
                    lines[i].Trim();

                bool isCommented =
                    trimmed.StartsWith(";");

                string settingLine =
                    isCommented
                        ? trimmed.Substring(1).Trim()
                        : trimmed;

                Match match =
                    Regex.Match(
                        settingLine,
                        @"^([a-zA-Z0-9_.-]+)\s*=");

                if (!match.Success)
                    continue;

                string settingName =
                    match.Groups[1].Value;

                if (!settingName.Equals(
                        name,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string currentSection =
                    FindSectionForLine(i);

                if (!currentSection.Equals(
                        section,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string indentation =
                    lines[i].Substring(
                        0,
                        lines[i].Length -
                        lines[i].TrimStart().Length);

                lines[i] =
                    $"{indentation}{name} = {FormatValue(value)}";

                return;
            }

            throw new InvalidOperationException(
                $"PHP setting '{name}' was not found in section '{section}'.");
        }

        public void SetEnabled(
            string section,
            string name,
            bool enabled)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed =
                    lines[i].Trim();

                bool isCommented =
                    trimmed.StartsWith(";");

                string settingLine =
                    isCommented
                        ? trimmed.Substring(1).Trim()
                        : trimmed;

                Match match =
                    Regex.Match(
                        settingLine,
                        @"^([a-zA-Z0-9_.-]+)\s*=\s*(.*)$");

                if (!match.Success)
                    continue;

                string settingName =
                    match.Groups[1].Value;

                if (!settingName.Equals(
                        name,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string currentSection =
                    FindSectionForLine(i);

                if (!currentSection.Equals(
                        section,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (enabled && isCommented)
                {
                    lines[i] =
                        lines[i].Replace(
                            ";",
                            "",
                            StringComparison.Ordinal);
                }
                else if (!enabled && !isCommented)
                {
                    string indentation =
                        lines[i].Substring(
                            0,
                            lines[i].Length -
                            lines[i].TrimStart().Length);

                    lines[i] =
                        $"{indentation};{lines[i].TrimStart()}";
                }

                return;
            }

            throw new InvalidOperationException(
                $"PHP setting '{name}' was not found in section '{section}'.");
        }

        private string FindSectionForLine(
            int lineIndex)
        {
            string currentSection =
                string.Empty;

            for (int i = 0;
                 i <= lineIndex && i < lines.Count;
                 i++)
            {
                string line =
                    lines[i].Trim();

                Match match =
                    Regex.Match(
                        line,
                        @"^\[(.+?)\]$");

                if (match.Success)
                {
                    currentSection =
                        match.Groups[1].Value.Trim();
                }
            }

            return currentSection;
        }

        private static string FormatValue(
            string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(
                    " ") ||
                value.Contains(
                    "\\"))
            {
                return $"\"{value}\"";
            }

            return value;
        }

        public void Save()
        {
            File.WriteAllLines(
                phpIniPath,
                lines);
        }

        public void Reload()
        {
            Load();
        }
    }
}