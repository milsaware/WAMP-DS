using System.IO;
using System.Windows;
using WAMP_DS.Managers;
using WAMP_DS.Core;
using System.Collections.Generic;

namespace WAMP_DS.Views
{
    public partial class DeveloperToolsWindow : Window
    {
        private readonly DeveloperToolsManager developerToolsManager;
        private readonly ApacheManager apacheManager;
        private readonly InstallationPaths installationPaths;
        private readonly MagentoManager magentoManager;

        private readonly Dictionary<string, string> testProgressLines = new();


        public DeveloperToolsWindow(
            DeveloperToolsManager developerToolsManager,
            ApacheManager apacheManager,
            InstallationPaths installationPaths,
            MagentoManager magentoManager)
        {
            InitializeComponent();

            this.developerToolsManager = developerToolsManager;
            this.apacheManager = apacheManager;
            this.installationPaths = installationPaths;
            this.magentoManager = magentoManager;

            this.magentoManager.OutputReceived +=
                MagentoManager_OutputReceived;
        }


        private void TestRewriteButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string apachePath =
                    installationPaths.ApachePath;


                string httpdConf =
                    Path.Combine(
                        apachePath,
                        "conf",
                        "httpd.conf"
                    );


                string message =
                    $"Apache Path:\n{apachePath}\n\n" +
                    $"Config File:\n{httpdConf}\n\n" +
                    $"Exists:\n{File.Exists(httpdConf)}\n\n";


                if (File.Exists(httpdConf))
                {
                    string content =
                        File.ReadAllText(httpdConf);


                    bool enabled =
                        content.Contains(
                            "LoadModule rewrite_module modules/mod_rewrite.so",
                            StringComparison.OrdinalIgnoreCase
                        );


                    bool commented =
                        content.Contains(
                            "# LoadModule rewrite_module modules/mod_rewrite.so",
                            StringComparison.OrdinalIgnoreCase
                        );


                    message +=
                        $"Enabled line found:\n{enabled}\n\n" +
                        $"Commented line found:\n{commented}\n\n";
                }


                apacheManager.EnableRewriteModule(
                    installationPaths.ApachePath
                );


                message +=
                    "\n\nMethod completed.";


                Clipboard.SetText(message);

                ReportBox.Text =
                    message;
            }
            catch (Exception ex)
            {
                ReportBox.Text =
                    ex.ToString();
            }
        }

        private void TestMagentoSourceMapPatchButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            try
            {
                MagentoPatchManager patchManager =
                    new MagentoPatchManager();

                string magentoPath =
                    @"D:\server\www\magento-seven";


                patchManager.PatchStaticContentSourceMaps(
                    magentoPath
                );


                ReportBox.Text =
                    "Magento source map references patched successfully.";
            }
            catch (Exception ex)
            {
                ReportBox.Text =
                    ex.ToString();
            }
        }

        private void TestMagentoContactPatchButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            try
            {
                string magentoPath =
                    @"D:\server\www\magento-six";


                MagentoPatchManager patchManager =
                    new MagentoPatchManager();


                patchManager.PatchContactLayout(magentoPath);


                ReportBox.Text =
                    "Magento Contact ViewModel patch applied.";
            }
            catch (Exception ex)
            {
                ReportBox.Text =
                    ex.ToString();
            }
        }

        private void MagentoManager_OutputReceived(
            object? sender,
            string message)
        {
            Dispatcher.Invoke(() =>
            {
                ReportBox.Text = message;
            });
        }

        private void TestMagentoStaticResourcePatchButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            try
            {
                string magentoPath =
                    @"D:\server\www\magento-seven";


                MagentoPatchManager patchManager =
                    new MagentoPatchManager();


                patchManager.PatchStaticResource(
                    magentoPath
                );


                ReportBox.Text =
                    "Magento StaticResource Windows patch applied successfully.";
            }
            catch (Exception ex)
            {
                ReportBox.Text =
                    ex.ToString();
            }
        }

        private void TestMagentoGeneratedFixButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            try
            {
                string projectPath =
                    @"D:\server\www\magento-seven";


                string generatedPath =
                    Path.Combine(
                        projectPath,
                        "generated"
                    );


                if (Directory.Exists(generatedPath))
                {
                    ReportBox.Text =
                        "Deleting generated folder...\n";


                    Directory.Delete(
                        generatedPath,
                        true
                    );


                    ReportBox.Text +=
                        "Generated folder deleted successfully.";
                }
                else
                {
                    ReportBox.Text =
                        "Generated folder does not exist.";
                }
            }
            catch (Exception ex)
            {
                ReportBox.Text =
                    ex.ToString();
            }
        }
    }
}