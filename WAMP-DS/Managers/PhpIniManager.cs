using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WAMP_DS.Managers
{
    public class PhpIniSetting
    {
        public string Section { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public bool IsEnabled { get; set; }

        public bool IsCommented { get; set; }

        public string OriginalLine { get; set; } = string.Empty;

        public int LineIndex { get; set; }

        public bool IsBoolean
        {
            get
            {
                string value =
                    Value.Trim()
                        .Trim('"')
                        .Trim('\'');

                return
                    value.Equals(
                        "On",
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    value.Equals(
                        "Off",
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    value.Equals(
                        "true",
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    value.Equals(
                        "false",
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    value == "1"
                    ||
                    value == "0";
            }
        }
    }


    public class PhpIniManager
    {
        private readonly string phpDirectory;

        private readonly string phpIniPath;


        public string PhpIniPath =>
            phpIniPath;


        public PhpIniManager()
        {
            phpDirectory =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "runtimes",
                    "php",
                    "8.5.8"
                );

            phpIniPath =
                Path.Combine(
                    phpDirectory,
                    "php.ini"
                );
        }


        // ============================================================
        // LOAD ALL SETTINGS
        // ============================================================

        public List<PhpIniSetting> LoadSettings()
        {
            var settings =
                new List<PhpIniSetting>();


            if (!File.Exists(phpIniPath))
            {
                return settings;
            }


            string currentSection =
                "Global";


            string[] lines =
                File.ReadAllLines(
                    phpIniPath
                );


            for (int i = 0;
                 i < lines.Length;
                 i++)
            {
                string line =
                    lines[i];


                string trimmed =
                    line.Trim();


                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }


                // ----------------------------------------------------
                // SECTION
                // ----------------------------------------------------

                if (trimmed.StartsWith("[")
                    &&
                    trimmed.EndsWith("]"))
                {
                    currentSection =
                        trimmed.Substring(
                            1,
                            trimmed.Length - 2
                        ).Trim();

                    continue;
                }


                // ----------------------------------------------------
                // IGNORE NON-SETTING LINES
                // ----------------------------------------------------

                string workingLine =
                    trimmed;


                bool commented =
                    workingLine.StartsWith(";");


                if (commented)
                {
                    workingLine =
                        workingLine.Substring(1)
                            .TrimStart();
                }


                if (!workingLine.Contains("="))
                {
                    continue;
                }


                string[] parts =
                    workingLine.Split(
                        '=',
                        2
                    );


                if (parts.Length != 2)
                {
                    continue;
                }


                string key =
                    parts[0].Trim();


                string value =
                    parts[1].Trim();


                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }


                settings.Add(
                    new PhpIniSetting
                    {
                        Section =
                            currentSection,

                        Key =
                            key,

                        Value =
                            value,

                        IsEnabled =
                            !commented,

                        IsCommented =
                            commented,

                        OriginalLine =
                            line,

                        LineIndex =
                            i
                    }
                );
            }


            return settings;
        }


        // ============================================================
        // GET SECTIONS
        // ============================================================

        public List<string> GetSections()
        {
            var sections =
                new List<string>();


            if (!File.Exists(phpIniPath))
            {
                return sections;
            }


            string currentSection =
                "Global";


            sections.Add(
                currentSection
            );


            foreach (string line in
                     File.ReadAllLines(phpIniPath))
            {
                string trimmed =
                    line.Trim();


                if (!trimmed.StartsWith("[")
                    ||
                    !trimmed.EndsWith("]"))
                {
                    continue;
                }


                string section =
                    trimmed.Substring(
                        1,
                        trimmed.Length - 2
                    ).Trim();


                if (!string.IsNullOrWhiteSpace(section))
                {
                    sections.Add(
                        section
                    );
                }
            }


            return sections
                .Distinct(
                    StringComparer.OrdinalIgnoreCase
                )
                .ToList();
        }


        // ============================================================
        // SAVE SETTINGS
        // ============================================================

        public void SaveSettings(
            IEnumerable<PhpIniSetting> settings)
        {
            if (!File.Exists(phpIniPath))
            {
                throw new FileNotFoundException(
                    "The PHP configuration file could not be found.",
                    phpIniPath
                );
            }


            string[] lines =
                File.ReadAllLines(
                    phpIniPath
                );


            foreach (PhpIniSetting setting in settings)
            {
                if (setting.LineIndex < 0
                    ||
                    setting.LineIndex >= lines.Length)
                {
                    continue;
                }


                string originalLine =
                    lines[
                        setting.LineIndex
                    ];


                string indentation =
                    originalLine
                        .Substring(
                            0,
                            originalLine.Length -
                            originalLine.TrimStart().Length
                        );


                string value =
                    setting.Value;


                if (setting.IsEnabled)
                {
                    lines[
                        setting.LineIndex
                    ] =
                        $"{indentation}{setting.Key} = {value}";
                }
                else
                {
                    lines[
                        setting.LineIndex
                    ] =
                        $"{indentation};{setting.Key} = {value}";
                }
            }


            File.WriteAllLines(
                phpIniPath,
                lines
            );
        }


        // ============================================================
        // GET SETTING
        // ============================================================

        public PhpIniSetting? GetSetting(
            string key)
        {
            return LoadSettings()
                .FirstOrDefault(
                    x =>
                        x.Key.Equals(
                            key,
                            StringComparison.OrdinalIgnoreCase
                        )
                );
        }


        // ============================================================
        // SET VALUE
        // ============================================================

        public void SetValue(
            string key,
            string value)
        {
            List<PhpIniSetting> settings =
                LoadSettings();


            PhpIniSetting? setting =
                settings.FirstOrDefault(
                    x =>
                        x.Key.Equals(
                            key,
                            StringComparison.OrdinalIgnoreCase
                        )
                );


            if (setting == null)
            {
                return;
            }


            setting.Value =
                value;


            setting.IsEnabled =
                true;


            SaveSettings(
                settings
            );
        }


        // ============================================================
        // ENABLE / DISABLE
        // ============================================================

        public void SetEnabled(
            string key,
            bool enabled)
        {
            List<PhpIniSetting> settings =
                LoadSettings();


            PhpIniSetting? setting =
                settings.FirstOrDefault(
                    x =>
                        x.Key.Equals(
                            key,
                            StringComparison.OrdinalIgnoreCase
                        )
                );


            if (setting == null)
            {
                return;
            }


            setting.IsEnabled =
                enabled;


            setting.IsCommented =
                !enabled;


            SaveSettings(
                settings
            );
        }

        public void EnableExtension(string extensionName)
        {
            if (!File.Exists(phpIniPath))
            {
                throw new FileNotFoundException(
                    "PHP ini file not found.",
                    phpIniPath
                );
            }

            string[] lines =
                File.ReadAllLines(phpIniPath);

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();

                if (trimmed == $";extension = {extensionName}" ||
                    trimmed == $";extension={extensionName}")
                {
                    lines[i] =
                        $"extension = {extensionName}";

                    break;
                }
            }

            File.WriteAllLines(
                phpIniPath,
                lines
            );
        }
    }
}