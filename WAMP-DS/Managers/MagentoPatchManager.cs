using System.IO;
using System.Text.RegularExpressions;

namespace WAMP_DS.Managers
{
    public class MagentoPatchManager
    {

        public void PatchContactLayout(string magentoPath)
        {
            string contactLayoutFile = Path.Combine(
                magentoPath,
                "vendor",
                "magento",
                "module-contact",
                "view",
                "frontend",
                "layout",
                "contact_index_index.xml"
            );

            if (!File.Exists(contactLayoutFile))
            {
                throw new FileNotFoundException(
                    "Magento contact layout file was not found.",
                    contactLayoutFile
                );
            }

            string content = File.ReadAllText(
                contactLayoutFile
            );

            string viewModelArgument =
                @"<argument name=""view_model"" xsi:type=""object"">
                    Magento\Contact\ViewModel\UserDataProvider
                </argument>";

            if (content.Contains(
                "Magento\\Contact\\ViewModel\\UserDataProvider",
                StringComparison.Ordinal
            ))
            {
                return;
            }

            string target = @"<argument name=""button_lock_manager"" xsi:type=""object"">Magento\Framework\View\Element\ButtonLockManager</argument>";

            if (!content.Contains(
                target,
                StringComparison.Ordinal
            ))
            {
                throw new InvalidOperationException(
                    "Magento contact button lock manager argument was not found. Patch not applied."
                );
            }

            content = content.Replace(
                target,
                target +
                Environment.NewLine +
                Environment.NewLine +
                "                    " +
                viewModelArgument,
                StringComparison.Ordinal
            );

            File.WriteAllText(
                contactLayoutFile,
                content
            );
        }

        public void PatchGd2(string magentoPath)
        {
            string gd2File = Path.Combine(
                magentoPath,
                "vendor",
                "magento",
                "framework",
                "Image",
                "Adapter",
                "Gd2.php"
            );

            if (!File.Exists(gd2File))
            {
                throw new FileNotFoundException(
                    "Magento GD2 adapter was not found.",
                    gd2File
                );
            }

            string content = File.ReadAllText(gd2File);

            string newMethod =
                @"private function validateURLScheme(string $filename) : bool
                {
                    if (preg_match('/^[A-Za-z]:[\/\\\\]/', $filename)) {
                        return true;
                    }

                    $allowed_schemes = ['ftp', 'ftps', 'http', 'https'];

                    $url = parse_url($filename);

                    if ($url && isset($url['scheme']) && !in_array($url['scheme'], $allowed_schemes)) {
                        return false;
                    }

                    return true;
                }";

            int start = content.IndexOf(
                "private function validateURLScheme",
                StringComparison.Ordinal
            );

            if (start == -1)
            {
                throw new InvalidOperationException(
                    "Magento GD2 validation method was not found. Patch not applied."
                );
            }

            int braceStart = content.IndexOf(
                '{',
                start
            );

            if (braceStart == -1)
            {
                throw new InvalidOperationException(
                    "Magento GD2 method opening brace was not found."
                );
            }

            int depth = 0;
            int end = -1;

            for (int i = braceStart; i < content.Length; i++)
            {
                if (content[i] == '{')
                {
                    depth++;
                }
                else if (content[i] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        end = i + 1;
                        break;
                    }
                }
            }

            if (end == -1)
            {
                throw new InvalidOperationException(
                    "Magento GD2 method closing brace was not found."
                );
            }

            content = content.Substring(0, start)
                + newMethod
                + content.Substring(end);

            File.WriteAllText(
                gd2File,
                content
            );
        }

        public void PatchPluginListGenerator(string magentoPath)
        {
            string pluginListFile = Path.Combine(
                magentoPath,
                "vendor",
                "magento",
                "framework",
                "Interception",
                "PluginListGenerator.php"
            );

            if (!File.Exists(pluginListFile))
            {
                throw new FileNotFoundException(
                    "Magento PluginListGenerator was not found.",
                    pluginListFile
                );
            }

            string content = File.ReadAllText(pluginListFile);

            string patchedLine = "$cacheId = implode('_', $this->scopePriorityScheme) . \"_\" . $this->cacheId;";

            if (content.Contains(
                patchedLine,
                StringComparison.Ordinal
            ))
            {
                return;
            }

            string originalLine = "$cacheId = implode('|', $this->scopePriorityScheme) . \"|\" . $this->cacheId;";

            if (!content.Contains(
                originalLine,
                StringComparison.Ordinal
            ))
            {
                throw new InvalidOperationException(
                    "Magento PluginListGenerator cache ID line was not found. Patch not applied."
                );
            }

            content = content.Replace(
                originalLine,
                patchedLine,
                StringComparison.Ordinal
            );

            File.WriteAllText(
                pluginListFile,
                content
            );
        }

        public void PatchStaticContentSourceMaps(string magentoPath)
        {
            string staticPath = Path.Combine(
                magentoPath,
                "pub",
                "static"
            );

            if (!Directory.Exists(staticPath))
            {
                return;
            }

            foreach (string jsFile in Directory.GetFiles(
                staticPath,
                "*.js",
                SearchOption.AllDirectories
            ))
            {
                string content = File.ReadAllText(
                    jsFile
                );

                if (!content.Contains(
                    "//# sourceMappingURL=",
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    continue;
                }

                string patchedContent = Regex.Replace(
                    content,
                    @"//# sourceMappingURL=.*\.map\s*$",
                    "",
                    RegexOptions.Multiline
                );

                if (patchedContent != content)
                {
                    File.WriteAllText(
                        jsFile,
                        patchedContent
                    );
                }
            }
        }

        public void PatchStaticResource(string magentoPath)
        {
            string staticResourceFile = Path.Combine(
                magentoPath,
                "vendor",
                "magento",
                "framework",
                "App",
                "StaticResource.php"
            );

            if (!File.Exists(staticResourceFile))
            {
                throw new FileNotFoundException(
                    "Magento StaticResource file was not found.",
                    staticResourceFile
                );
            }

            string content = File.ReadAllText(staticResourceFile);

            string patchedBlock =
            @"if (
                !(
                    $this->isThemeAllowed(
                        str_replace(
                            '\\',
                            '/',
                            $params['area'] . DIRECTORY_SEPARATOR . $params['theme']
                        )
                    )
                    && $this->localeValidator->isValid($params['locale'])
                )
            )";

            if (content.Contains(
                "str_replace(",
                StringComparison.Ordinal
            ))
            {
                return;
            }

            string pattern =
                @"if\s*\(\s*!\s*\(\s*\$this->isThemeAllowed\(\$params\['area'\]\s*\.\s*DIRECTORY_SEPARATOR\s*\.\s*\$params\['theme'\]\)\s*&&\s*\$this->localeValidator->isValid\(\$params\['locale'\]\)\s*\)\s*\)";

            Match match = Regex.Match(
                content,
                pattern,
                RegexOptions.Multiline
            );

            if (!match.Success)
            {
                throw new InvalidOperationException(
                    "Magento StaticResource theme validation block was not found. Patch not applied."
                );
            }

            content = content.Replace(
                match.Value,
                patchedBlock,
                StringComparison.Ordinal
            );

            File.WriteAllText(
                staticResourceFile,
                content
            );
        }

        public void PatchTemplateValidator(string magentoPath)
        {
            string validatorFile = Path.Combine(
                magentoPath,
                "vendor",
                "magento",
                "framework",
                "View",
                "Element",
                "Template",
                "File",
                "Validator.php"
            );

            if (!File.Exists(validatorFile))
            {
                throw new FileNotFoundException(
                    "Magento template validator was not found.",
                    validatorFile
                );
            }

            string content = File.ReadAllText(
                validatorFile
            );

            string newMethod = @"protected function isPathInDirectories($path, $directories)
            {
                if (preg_match('/^[A-Za-z]:[\/\\\\]/', $path)) {
                    $path = str_replace('\\', '/', $path);
                }

                if (!is_array($directories)) {
                    $directories = (array)$directories;
                }

                $realPath = str_replace(
                    '\\',
                    '/',
                    $this->fileDriver->getRealPath($path)
                );

                foreach ($directories as $directory) {
                    if ($directory !== null) {

                        $directory = str_replace(
                            '\\',
                            '/',
                            $directory
                        );

                        if (0 === strpos($realPath, $directory)) {
                            return true;
                        }
                    }
                }

                return false;
            }";

            content = ReplaceMagentoMethod(
                content,
                "protected function isPathInDirectories",
                newMethod
            );

            File.WriteAllText(
                validatorFile,
                content
            );
        }

        private static string ReplaceMagentoMethod(
            string content,
            string methodName,
            string replacement)
        {
            int start = content.IndexOf(methodName);

            if (start == -1)
                throw new InvalidOperationException(
                    "Magento method was not found."
                );

            int braceStart = content.IndexOf(
                '{',
                start
            );

            if (braceStart == -1)
                throw new InvalidOperationException(
                    "Magento method opening brace was not found."
                );

            int depth = 0;
            int end = -1;

            for (int i = braceStart; i < content.Length; i++)
            {
                if (content[i] == '{')
                {
                    depth++;
                }
                else if (content[i] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        end = i + 1;
                        break;
                    }
                }
            }

            if (end == -1)
                throw new InvalidOperationException(
                    "Magento method closing brace was not found."
                );

            return
                content.Remove(
                    start,
                    end - start
                )
                .Insert(
                    start,
                    replacement
                );
        }
    }
}