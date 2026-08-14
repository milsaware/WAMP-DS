using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WAMP_DS.Managers;
using WAMP_DS.Models;

namespace WAMP_DS.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly ApacheManager apacheManager;
        private readonly MySQLManager mySQLManager;
        private readonly MySQLSettingsManager mySQLSettingsManager;
        private readonly DatabaseSettingsManager databaseSettingsManager;
        private readonly CertificateManager certificateManager;
        private readonly string phpDirectory;

        public SettingsWindow(
            ApacheManager apacheManager,
            MySQLManager mySQLManager,
            MySQLSettingsManager mySQLSettingsManager)
        {
            InitializeComponent();

            this.apacheManager =
                apacheManager;

            this.mySQLManager =
                mySQLManager;

            databaseSettingsManager =
                new DatabaseSettingsManager();

            this.mySQLSettingsManager =
                mySQLSettingsManager;

            phpDirectory =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "runtimes",
                    "php",
                    "8.5.8"
                );

            certificateManager =
                new CertificateManager();

            CertificateFileText.Text =
                GetRelativeApachePath(
                    certificateManager.ServerCertificate
                );

            PrivateKeyFileText.Text =
                GetRelativeApachePath(
                    certificateManager.ServerPrivateKey
                );

            CertificateFullPathText.Text =
                certificateManager.ServerCertificate;

            PrivateKeyFullPathText.Text =
                certificateManager.ServerPrivateKey;

            apacheManager.StatusChanged +=
    ApacheManager_StatusChanged;

            UpdateHttpsStatus();
            UpdateApacheSettings();
        }

        private void ApacheManager_StatusChanged(
    object? sender,
    EventArgs e)
        {
            Dispatcher.Invoke(
                UpdateApacheSettings
            );
        }

        private string GetRelativeApachePath(
            string fullPath)
        {
            string apacheRoot =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "runtimes",
                    "apache",
                    "2.4.68"
                );

            return Path.GetRelativePath(
                apacheRoot,
                fullPath
            );
        }

        private void CertificateFileText_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            OpenFileLocation(
                certificateManager.ServerCertificate
            );
        }

        private void PrivateKeyFileText_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            OpenFileLocation(
                certificateManager.ServerPrivateKey
            );
        }

        private void OpenFileLocation(
            string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show(
                    "The requested certificate file could not be found.",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{filePath}\"",
                    UseShellExecute = true
                }
            );
        }

        private void UpdateHttpsStatus()
        {
            bool httpsEnabled =
                apacheManager.IsHttpsEnabled();

            HttpsStatusText.Text =
                httpsEnabled
                    ? "Enabled"
                    : "Disabled";

            HttpsControlButton.Content =
                httpsEnabled
                    ? "Disable HTTPS"
                    : "Enable HTTPS";

            HttpsDetailsPanel.Visibility =
                httpsEnabled
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            if (httpsEnabled)
            {
                UpdateCertificateDetails();
            }
        }

        private void UpdateCertificateDetails()
        {
            X509Certificate2? certificate =
                certificateManager.GetServerCertificate();

            X509Certificate2? rootCertificate =
                certificateManager.GetRootCertificate();

            if (certificate == null)
            {
                CertificateStatusText.Text =
                    "Certificate not found";

                CertificateStatusText.Foreground =
                    System.Windows.Media.Brushes.IndianRed;

                return;
            }

            CertificateSubjectText.Text =
                certificate.GetNameInfo(
                    X509NameType.DnsName,
                    false
                );

            CertificateIssuerText.Text =
                rootCertificate?.GetNameInfo(
                    X509NameType.SimpleName,
                    false
                ) ?? "Unknown";

            CertificateValidFromText.Text =
                certificate.NotBefore.ToLocalTime()
                    .ToString(
                        "dd/MM/yyyy HH:mm"
                    );

            CertificateValidUntilText.Text =
                certificate.NotAfter.ToLocalTime()
                    .ToString(
                        "dd/MM/yyyy HH:mm"
                    );

            CertificateSanText.Text =
                "localhost, 127.0.0.1, ::1";

            bool trusted =
                certificateManager.IsServerCertificateTrusted();

            CertificateStatusText.Text =
                trusted
                    ? "✓ Trusted"
                    : "✕ Not Trusted";

            CertificateStatusText.Foreground =
                trusted
                    ? System.Windows.Media.Brushes.LightGreen
                    : System.Windows.Media.Brushes.IndianRed;

            CertificateFileText.Text =
                GetRelativeApachePath(
                    certificateManager.ServerCertificate
                );

            PrivateKeyFileText.Text =
                GetRelativeApachePath(
                    certificateManager.ServerPrivateKey
                );

            CertificateFullPathText.Text =
                certificateManager.ServerCertificate;

            PrivateKeyFullPathText.Text =
                certificateManager.ServerPrivateKey;
        }


        // ============================================================
        // SETTINGS NAVIGATION
        // ============================================================

        private void ShowSettingsPanel(
            Grid panel,
            Button selectedButton)
        {
            GeneralSettingsPanel.Visibility =
                Visibility.Collapsed;

            ApacheSettingsPanel.Visibility =
                Visibility.Collapsed;

            PhpSettingsPanel.Visibility =
                Visibility.Collapsed;

            MySqlSettingsPanel.Visibility =
                Visibility.Collapsed;

            HttpsSettingsPanel.Visibility =
                Visibility.Collapsed;


            GeneralSettingsButton.Background =
                System.Windows.Media.Brushes.Transparent;

            ApacheSettingsButton.Background =
                System.Windows.Media.Brushes.Transparent;

            PhpSettingsButton.Background =
                System.Windows.Media.Brushes.Transparent;

            MySqlSettingsButton.Background =
                System.Windows.Media.Brushes.Transparent;

            HttpsSettingsButton.Background =
                System.Windows.Media.Brushes.Transparent;

            panel.Visibility = Visibility.Visible;

            selectedButton.Background =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(
                        45,
                        45,
                        48
                    )
                );
        }


        // ============================================================
        // GENERAL
        // ============================================================

        private void GeneralSettingsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowSettingsPanel(
                GeneralSettingsPanel,
                GeneralSettingsButton
            );
        }


        // ============================================================
        // APACHE
        // ============================================================

        private void ApacheSettingsButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            ShowSettingsPanel(
                ApacheSettingsPanel,
                ApacheSettingsButton
            );

            UpdateApacheSettings();
        }


        // ============================================================
        // PHP
        // ============================================================

        private void PhpSettingsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowSettingsPanel(
                PhpSettingsPanel,
                PhpSettingsButton
            );
        }


        // ============================================================
        // MYSQL
        // ============================================================

        private void MySqlSettingsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowSettingsPanel(
                MySqlSettingsPanel,
                MySqlSettingsButton
            );

            UpdateMySQLSettings();
        }


        // ============================================================
        // HTTPS / SSL
        // ============================================================

        private void HttpsSettingsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowSettingsPanel(
                HttpsSettingsPanel,
                HttpsSettingsButton
            );

            UpdateHttpsStatus();
        }


        private async void HttpsControlButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (apacheManager.IsHttpsEnabled())
                {
                    apacheManager.DisableHttps();
                }
                else
                {
                    apacheManager.EnableHttps();
                }

                UpdateHttpsStatus();

                if (apacheManager.IsRunning)
                {
                    apacheManager.Stop();

                    await apacheManager.StartAsync();

                    UpdateHttpsStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to change HTTPS configuration.\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                UpdateHttpsStatus();
            }
        }

        private void OpenPhpSettingsButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            PhpSettingsWindow phpSettingsWindow =
                new PhpSettingsWindow(
                    apacheManager,
                    mySQLManager,
                    phpDirectory
                );

            phpSettingsWindow.Owner =
                this;

            phpSettingsWindow.ShowDialog();
        }

        private void OpenApacheConfigButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            ApacheConfigEditorWindow window =
                new ApacheConfigEditorWindow(
                    apacheManager,
                    apacheManager.ConfigurationFile,
                    "httpd.conf"
                );

            window.Owner = this;
            window.ShowDialog();
        }

        private void OpenApacheModulesButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            ApacheSettingsWindow apacheSettingsWindow =
                new ApacheSettingsWindow(
                    apacheManager
                );

            apacheSettingsWindow.Owner =
                this;

            apacheSettingsWindow.ShowDialog();
        }

        private void OpenApacheVirtualHostsButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            ApacheVhostsWindow window =
                new ApacheVhostsWindow(
                    apacheManager
                );

            window.Owner =
                this;

            window.ShowDialog();
        }

        private void OpenApacheLogsButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            LogViewerWindow window =
                new LogViewerWindow(
                    apacheManager.ApacheDirectory
                );

            window.Owner = this;

            window.Show();
        }

        private void UpdateApacheSettings()
        {
            ApacheVersionText.Text =
                $"Apache {apacheManager.Version}";


            bool running =
                apacheManager.Status == ApacheStatus.Running;


            ApacheStatusText.Text =
                running
                    ? "RUNNING"
                    : "STOPPED";


            ApacheStatusText.Foreground =
                running
                    ? System.Windows.Media.Brushes.LightGreen
                    : System.Windows.Media.Brushes.IndianRed;


            ApacheHttpPortText.Text =
                apacheManager.Port.ToString();


            ApacheHttpsPortSettingsText.Text =
                apacheManager.IsHttpsEnabled()
                    ? apacheManager.HttpsPort.ToString()
                    : "Disabled";
        }

        private void UpdateMySQLSettings()
        {
            MySqlVersionText.Text =
                $"MySQL {mySQLManager.Version}";

            MySqlPortText.Text =
                mySQLManager.Port.ToString();

            MySqlStatusText.Text =
                mySQLManager.Status.ToString().ToUpper();


            DatabaseCredentials credentials =
                databaseSettingsManager.Load();


            MySqlHostText.Text =
                credentials.Host;


            MySqlUsernameText.Text =
                credentials.Username;


            MySqlPasswordText.Password =
                credentials.Password;
        }

        private void SaveMySqlSettingsButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            DatabaseCredentials credentials = new()
            {
                Host = MySqlHostText.Text,
                Port = mySQLManager.Port,
                Username = MySqlUsernameText.Text,
                Password = MySqlPasswordText.Password
            };


            databaseSettingsManager.Save(
                credentials
            );


            MessageBox.Show(
                "MySQL settings saved.",
                "WAMP-DS",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}