using System.Windows;
using System.IO;
using WAMP_DS.Installer.Models;
using WAMP_DS.Installer.Managers;

namespace WAMP_DS.Installer
{
    public partial class MainWindow : Window
    {
        private readonly InstallationOptions _options = new();
        private InstallationManager? _installationManager;
        private CancellationTokenSource? _installationCancellation;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        private void NextButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            WelcomePage.Visibility = Visibility.Collapsed;
            LocationPage.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LocationPage.Visibility = Visibility.Collapsed;
            WelcomePage.Visibility = Visibility.Visible;
        }

        private void BrowseButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            using System.Windows.Forms.FolderBrowserDialog dialog = new();

            dialog.Description =
                "Choose where WAMP-DS should be installed.";

            dialog.UseDescriptionForTitle = true;

            if (dialog.ShowDialog() ==
                System.Windows.Forms.DialogResult.OK)
            {
                InstallationPathTextBox.Text =
                    dialog.SelectedPath;
            }
        }

        private async void LocationNextButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            string path =
                InstallationPathTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(path))
            {
                System.Windows.MessageBox.Show(
                    "Please choose an installation location.",
                    "WAMP-DS Installer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                string fullPath =
                    Path.GetFullPath(path);

                _options.InstallationPath =
                    fullPath;

                InstallationPathTextBox.Text =
                    _options.InstallationPath;
            }
            catch
            {
                System.Windows.MessageBox.Show(
                    "The installation location is not a valid Windows path.",
                    "WAMP-DS Installer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            LocationPage.Visibility =
                Visibility.Collapsed;

            InstallationPage.Visibility =
                Visibility.Visible;

            await StartInstallationAsync();
        }

        private async Task StartInstallationAsync()
        {
            DetailsBorder.Visibility = Visibility.Collapsed;

            DetailsButton.Content = "Show Details";

            InstallationReportTextBox.Clear();

            _installationCancellation =
                new CancellationTokenSource();

            InstallationCancelButton.Content =
                "Cancel";

            InstallationCancelButton.IsEnabled =
                true;

            _installationManager =
                new InstallationManager(_options);

            Progress<InstallationProgress> progress =
    new(progress =>
    {
        if (!string.IsNullOrWhiteSpace(progress.Message))
        {
            InstallationStatusText.Text =
                progress.Message;

            InstallationReportTextBox.AppendText(
                progress.Message +
                Environment.NewLine);

            InstallationReportTextBox.ScrollToEnd();
        }

        InstallationProgressBar.Value =
            progress.Percentage;

        InstallationReportTextBox.ScrollToEnd();
    });

            try
            {
                await _installationManager.InstallAsync(
                    progress,
                    _installationCancellation.Token);

                InstallationStatusText.Text =
                    "Installation complete.";

                InstallationProgressBar.Value =
                    100;

                InstallationCancelButton.Visibility =
                    Visibility.Collapsed;

                LaunchWampDsCheckBox.Visibility =
                    Visibility.Visible;

                FinishButton.Visibility =
                    Visibility.Visible;
            }
            catch (OperationCanceledException)
            {
                _installationCancellation?.Dispose();
                _installationCancellation = null;

                InstallationPage.Visibility =
                    Visibility.Collapsed;

                WelcomePage.Visibility =
                    Visibility.Visible;
            }
            catch (Exception ex)
            {
                InstallationStatusText.Text =
                    "Installation failed.";

                InstallationCancelButton.Content =
                    "Close";

                System.Windows.MessageBox.Show(
                    ex.Message,
                    "WAMP-DS Installer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void InstallationCancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (InstallationCancelButton.Content?.ToString() == "Close")
            {
                Close();
                return;
            }

            _installationCancellation?.Cancel();
        }

        private void DetailsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DetailsBorder.Visibility ==
                Visibility.Visible)
            {
                DetailsBorder.Visibility =
                    Visibility.Collapsed;

                DetailsButton.Content =
                    "Show Details";
            }
            else
            {
                DetailsBorder.Visibility =
                    Visibility.Visible;

                DetailsButton.Content =
                    "Hide Details";
            }
        }

        private void FinishButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (LaunchWampDsCheckBox.IsChecked == true)
            {
                string executablePath =
                    Path.Combine(
                        _options.InstallationPath,
                        "WAMP-DS.exe");

                if (File.Exists(executablePath))
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = executablePath,
                            UseShellExecute = true
                        });
                }
            }

            Close();
        }
    }
}