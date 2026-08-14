using WAMP_DS.Models;
using WAMP_DS.Core;
using System.IO;
using System.Text.Json;

namespace WAMP_DS.Managers
{
    public class ProjectCreationManager
    {
        private readonly ProjectManager projectManager;
        private readonly ApacheManager apacheManager;
        private readonly LaravelManager laravelManager;
        private readonly WordPressManager wordpressManager;
        private readonly MagentoManager magentoManager;
        private readonly CodeIgniterManager codeIgniterManager;
        private readonly DatabaseManager databaseManager;
        private readonly MySQLSettingsManager mysqlSettingsManager;

        public event EventHandler<string>? ProgressChanged;

        public ProjectCreationManager(
            ProjectManager projectManager,
            ApacheManager apacheManager,
            DatabaseManager databaseManager,
            MySQLSettingsManager mysqlSettingsManager,
            InstallationPaths installationPaths)
        {
            this.databaseManager = databaseManager;
            this.projectManager = projectManager;
            this.apacheManager = apacheManager;
            this.mysqlSettingsManager = mysqlSettingsManager;

            laravelManager =
                new LaravelManager(
                    databaseManager,
                    mysqlSettingsManager
                );

            laravelManager.OutputReceived +=
                (sender, message) =>
                {
                    Report(message);
                };

            magentoManager =
                new MagentoManager(
                    installationPaths
                );

            magentoManager.OutputReceived +=
                (sender, message) =>
                {
                    Report(message);
                };

            wordpressManager =
                new WordPressManager();


            wordpressManager.OutputReceived +=
                (sender, message) =>
                {
                    Report(message);
                };

            codeIgniterManager =
                new CodeIgniterManager(
                    installationPaths,
                    databaseManager
                );

            codeIgniterManager.OutputReceived +=
                (sender, message) =>
                {
                    Report(message);
                };
        }

        private void Report(string message)
        {
            ProgressChanged?.Invoke(
                this,
                message
            );
        }

        public async Task<bool> CreateProject(
            ProjectCreationOptions options)
        {
            string? projectPath = null;
            MagentoInstallSettings? installSettings = null;
            WordPressInstallSettings? wordpressInstallSettings = null;

            try
            {
                if (options.ProjectType == ProjectType.Laravel)
                {
                    Report("Building Laravel project...");

                    projectPath = await laravelManager.InstallLaravel(
                        options.ParentDirectory,
                        options.ProjectName,
                        options
                    );

                    if (projectPath != null)
                    {
                        projectManager.OpenProject(
                            projectPath
                        );
                    }
                }
                else if (options.ProjectType == ProjectType.WordPress)
                {
                    Report("Building WordPress project...");

                    wordpressInstallSettings = await wordpressManager.GetInstallationSettings();

                    if (wordpressInstallSettings == null)
                    {
                        return false;
                    }

                    projectPath = await wordpressManager.InstallWordPress(
                        options.ParentDirectory,
                        options.ProjectName,
                        wordpressInstallSettings
                    );

                    if (projectPath != null)
                    {
                        projectManager.OpenProject(
                            projectPath
                        );
                    }
                }
                else if (options.ProjectType == ProjectType.Magento)
                {
                    Report("Building Magento project...");

                    installSettings = magentoManager.GetInstallationSettings(
                        options.VirtualHostDomain
                    );

                    if (installSettings == null)
                    {
                        return false;
                    }

                    projectPath = await magentoManager.InstallMagento(
                        options.ParentDirectory,
                        options.ProjectName,
                        installSettings
                    );

                    if (projectPath != null)
                    {
                        projectManager.OpenProject(
                            projectPath
                        );
                    }
                }
                else if (options.ProjectType == ProjectType.CodeIgniter)
                {
                    Report("Building CodeIgniter project...");

                    projectPath =
                        Path.Combine(
                            options.ParentDirectory,
                            options.ProjectName
                        );

                    Directory.CreateDirectory(
                        projectPath
                    );

                    bool installed =
                        await codeIgniterManager.InstallCodeIgniter(
                            projectPath,
                            options
                        );

                    if (!installed)
                    {
                        return false;
                    }

                    if (projectPath != null)
                    {
                        projectManager.OpenProject(
                            projectPath
                        );
                    }
                }
                else
                {
                    Report("Creating project files...");

                    projectPath = projectManager.CreateProject(
                        options
                    );
                }

                if (projectPath == null)
                {
                    Report("Project creation failed.");

                    return false;
                }

                Report("Creating WAMP-DS project settings...");

                await CreateWampDsSettings(
                    projectPath,
                    options
                );

                if (options.ProjectType == ProjectType.WordPress)
                {
                    if (wordpressInstallSettings == null)
                    {
                        Report("WordPress installation settings missing.");

                        return false;
                    }

                    bool installed = await wordpressManager.CompleteInstallation(
                        projectPath,
                        options.DatabaseName,
                        options.VirtualHostDomain,
                        wordpressInstallSettings
                    );

                    if (!installed)
                    {
                        return false;
                    }
                }

                Report("WAMP-DS project settings created.");

                if (options.ProjectType == ProjectType.Magento)
                {
                    bool installed = await magentoManager.CompleteInstallation(
                        projectPath,
                        options.DatabaseName,
                        installSettings
                    );

                    if (!installed)
                    {
                        return false;
                    }
                }

                Report("Project files created.");

                if (options.CreateVirtualHost)
                {
                    string documentRoot = projectPath;

                    if (options.ProjectType == ProjectType.Laravel ||
    options.ProjectType == ProjectType.CodeIgniter)
                    {
                        Report("Setting framework document root...");

                        documentRoot = Path.Combine(
                            projectPath,
                            "public"
                        );

                        if (!Directory.Exists(documentRoot))
                        {
                            Report("ERROR: Public folder missing.");

                            return false;
                        }
                    }
                    else if (options.ProjectType == ProjectType.Magento)
                    {
                        documentRoot = magentoManager.GetDocumentRoot(
                            projectPath
                        );
                    }

                    Report("Creating Apache virtual host...");

                    await apacheManager.CreateProjectVirtualHost(
                        options.ProjectName,
                        documentRoot,
                        options.VirtualHostDomain
                    );
                    
                    Report("Apache virtual host created.");
                }

                Report("Project creation complete.");

                return true;
            }
            catch (Exception ex)
            {
                Report($"ERROR: {ex.Message}");

                return false;
            }
        }

        private async Task CreateWampDsSettings(
            string projectPath,
            ProjectCreationOptions options)
        {
            string settingsPath =
                Path.Combine(
                    projectPath,
                    "settings.wampds"
                );

            var settings = new
            {
                domain = options.VirtualHostDomain,
                ssl = options.EnableHttps,
                projectName = options.ProjectName
            };

            string json =
                System.Text.Json.JsonSerializer.Serialize(
                    settings,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );

            await File.WriteAllTextAsync(
                settingsPath,
                json
            );
        }
    }
}