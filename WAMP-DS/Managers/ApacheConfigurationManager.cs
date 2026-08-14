using System.IO;

namespace WAMP_DS.Managers
{
    public class ApacheConfigurationManager
    {
        private readonly string apacheConfig;

        public ApacheConfigurationManager(
            string apacheConfig)
        {
            this.apacheConfig =
                apacheConfig;
        }

        public bool IsModuleEnabled(
            string moduleName)
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            string modulePrefix =
                $"LoadModule {moduleName} ";

            foreach (string line in lines)
            {
                string trimmed =
                    line.TrimStart();

                if (trimmed.StartsWith(
                    modulePrefix,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool EnableModule(
            string moduleName)
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            string modulePrefix =
                $"LoadModule {moduleName} ";

            for (int i = 0;
                i < lines.Length;
                i++)
            {
                string trimmed =
                    lines[i].TrimStart();

                if (trimmed.StartsWith(
                    modulePrefix,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (trimmed.StartsWith(
                    $"# {modulePrefix}",
                    StringComparison.OrdinalIgnoreCase))
                {
                    int indentation =
                        lines[i].Length -
                        lines[i].TrimStart().Length;

                    string prefix =
                        new string(
                            ' ',
                            indentation
                        );

                    lines[i] =
                        prefix +
                        trimmed.Substring(2);

                    File.WriteAllLines(
                        apacheConfig,
                        lines
                    );

                    return true;
                }

                if (trimmed.StartsWith(
                    $"#{modulePrefix}",
                    StringComparison.OrdinalIgnoreCase))
                {
                    int indentation =
                        lines[i].Length -
                        lines[i].TrimStart().Length;

                    string prefix =
                        new string(
                            ' ',
                            indentation
                        );

                    lines[i] =
                        prefix +
                        trimmed.Substring(1);

                    File.WriteAllLines(
                        apacheConfig,
                        lines
                    );

                    return true;
                }
            }

            throw new InvalidOperationException(
                $"The Apache configuration does not contain a LoadModule entry for '{moduleName}'."
            );
        }

        public bool DisableModule(
            string moduleName)
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            string modulePrefix =
                $"LoadModule {moduleName} ";

            for (int i = 0;
                i < lines.Length;
                i++)
            {
                string trimmed =
                    lines[i].TrimStart();

                if (trimmed.StartsWith(
                    $"# {modulePrefix}",
                    StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith(
                    $"#{modulePrefix}",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (trimmed.StartsWith(
                    modulePrefix,
                    StringComparison.OrdinalIgnoreCase))
                {
                    int indentation =
                        lines[i].Length -
                        lines[i].TrimStart().Length;

                    string prefix =
                        new string(
                            ' ',
                            indentation
                        );

                    lines[i] =
                        prefix +
                        "# " +
                        trimmed;

                    File.WriteAllLines(
                        apacheConfig,
                        lines
                    );

                    return true;
                }
            }

            throw new InvalidOperationException(
                $"The Apache configuration does not contain a LoadModule entry for '{moduleName}'."
            );
        }

        public bool IsIncludeEnabled(
            string includePath)
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            string includeDirective =
                $"Include {includePath}";

            foreach (string line in lines)
            {
                string trimmed =
                    line.TrimStart();

                if (trimmed.Equals(
                    includeDirective,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool EnableInclude(
            string includePath)
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            string includeDirective =
                $"Include {includePath}";

            for (int i = 0;
                i < lines.Length;
                i++)
            {
                string trimmed =
                    lines[i].TrimStart();

                if (trimmed.Equals(
                    includeDirective,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (trimmed.Equals(
                    $"# {includeDirective}",
                    StringComparison.OrdinalIgnoreCase))
                {
                    int indentation =
                        lines[i].Length -
                        lines[i].TrimStart().Length;

                    string prefix =
                        new string(
                            ' ',
                            indentation
                        );

                    lines[i] =
                        prefix +
                        trimmed.Substring(2);

                    File.WriteAllLines(
                        apacheConfig,
                        lines
                    );

                    return true;
                }

                if (trimmed.Equals(
                    $"#{includeDirective}",
                    StringComparison.OrdinalIgnoreCase))
                {
                    int indentation =
                        lines[i].Length -
                        lines[i].TrimStart().Length;

                    string prefix =
                        new string(
                            ' ',
                            indentation
                        );

                    lines[i] =
                        prefix +
                        trimmed.Substring(1);

                    File.WriteAllLines(
                        apacheConfig,
                        lines
                    );

                    return true;
                }
            }

            throw new InvalidOperationException(
                $"The Apache configuration does not contain an Include directive for '{includePath}'."
            );
        }

        public bool DisableInclude(
            string includePath)
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            string includeDirective =
                $"Include {includePath}";

            for (int i = 0;
                i < lines.Length;
                i++)
            {
                string trimmed =
                    lines[i].TrimStart();

                if (trimmed.Equals(
                    $"# {includeDirective}",
                    StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Equals(
                    $"#{includeDirective}",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (trimmed.Equals(
                    includeDirective,
                    StringComparison.OrdinalIgnoreCase))
                {
                    int indentation =
                        lines[i].Length -
                        lines[i].TrimStart().Length;

                    string prefix =
                        new string(
                            ' ',
                            indentation
                        );

                    lines[i] =
                        prefix +
                        "# " +
                        trimmed;

                    File.WriteAllLines(
                        apacheConfig,
                        lines
                    );

                    return true;
                }
            }

            throw new InvalidOperationException(
                $"The Apache configuration does not contain an Include directive for '{includePath}'."
            );
        }

        public bool SetDefine(
            string defineName,
            string value)
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            string definePrefix =
                $"Define {defineName} ";

            for (int i = 0;
                i < lines.Length;
                i++)
            {
                string trimmed =
                    lines[i].TrimStart();

                if (!trimmed.StartsWith(
                    definePrefix,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string newLine =
                    $"Define {defineName} \"{value}\"";

                if (string.Equals(
                    lines[i],
                    newLine,
                    StringComparison.Ordinal))
                {
                    return false;
                }

                int indentation =
                    lines[i].Length -
                    lines[i].TrimStart().Length;

                string prefix =
                    new string(
                        ' ',
                        indentation
                    );

                lines[i] =
                    prefix +
                    newLine;

                File.WriteAllLines(
                    apacheConfig,
                    lines
                );

                return true;
            }

            throw new InvalidOperationException(
                $"The Apache configuration does not contain a Define directive for '{defineName}'."
            );
        }

        public bool SetDirective(
            string directiveName,
            string value)
        {
            string[] lines =
                File.ReadAllLines(
                    apacheConfig
                );

            string directivePrefix =
                $"{directiveName} ";

            for (int i = 0;
                i < lines.Length;
                i++)
            {
                string trimmed =
                    lines[i].TrimStart();

                if (!trimmed.StartsWith(
                    directivePrefix,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string newLine =
                    $"{directiveName} \"{value}\"";

                if (string.Equals(
                    lines[i],
                    newLine,
                    StringComparison.Ordinal))
                {
                    return false;
                }

                int indentation =
                    lines[i].Length -
                    lines[i].TrimStart().Length;

                string prefix =
                    new string(
                        ' ',
                        indentation
                    );

                lines[i] =
                    prefix +
                    newLine;

                File.WriteAllLines(
                    apacheConfig,
                    lines
                );

                return true;
            }

            throw new InvalidOperationException(
                $"The Apache configuration does not contain a '{directiveName}' directive."
            );
        }
    }
}