using System.Diagnostics;
using System.IO;
using System.Windows;

namespace WAMP_DS.Uninstaller
{
    public partial class MainWindow : Window
    {
        private readonly string _installationPath;

        public MainWindow()
        {
            InitializeComponent();

            _installationPath =
                Directory.GetParent(
                    AppContext.BaseDirectory.TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar))!
                .FullName;
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        private async void UninstallButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBoxResult result =
                MessageBox.Show(
                    "Are you sure you want to uninstall WAMP-DS?",
                    "WAMP-DS Uninstaller",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            UninstallButton.IsEnabled = false;
            CancelButton.IsEnabled = false;

            OutputTextBox.Clear();

            WriteOutput(
                "Preparing uninstall...");

            try
            {
                await PerformUninstallAsync();
            }
            catch (Exception ex)
            {
                WriteOutput("");
                WriteOutput(
                    $"ERROR: {ex.Message}");

                WriteOutput("");
                WriteOutput(
                    "Uninstall failed.");

                CancelButton.IsEnabled = true;
                UninstallButton.IsEnabled = true;
            }
        }

        private async Task PerformUninstallAsync()
        {
            string desktopShortcut =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.DesktopDirectory),
                    "WAMP-DS.lnk");

            string startMenuDirectory =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.StartMenu),
                    "Programs",
                    "WAMP-DS");

            string startMenuShortcut =
                Path.Combine(
                    startMenuDirectory,
                    "WAMP-DS.lnk");

            await Task.Delay(400);

            WriteOutput(
                "Removing Desktop shortcut...");

            if (File.Exists(desktopShortcut))
            {
                File.Delete(desktopShortcut);
            }

            await Task.Delay(300);

            WriteOutput(
                "Removing Start Menu shortcut...");

            if (File.Exists(startMenuShortcut))
            {
                File.Delete(startMenuShortcut);
            }

            await Task.Delay(300);

            WriteOutput(
                "Removing Start Menu folder...");

            if (Directory.Exists(startMenuDirectory))
            {
                Directory.Delete(
                    startMenuDirectory,
                    true);
            }

            await Task.Delay(300);

            WriteOutput(
                "Removing Windows application registration...");

            RemoveWindowsRegistration();

            await Task.Delay(300);

            WriteOutput(
                "Preparing final cleanup...");

            await Task.Delay(500);

            WriteOutput(
                "Uninstall complete.");

            await Task.Delay(700);

            StartFinalCleanup();
        }

        private static void RemoveWindowsRegistration()
        {
            try
            {
                using Microsoft.Win32.RegistryKey? key =
                    Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                        @"Software\Microsoft\Windows\CurrentVersion\Uninstall",
                        true);

                key?.DeleteSubKeyTree(
                    "WAMP-DS",
                    false);
            }
            catch
            {
                // Registration may already be absent.
            }
        }

        private void StartFinalCleanup()
        {
            string scriptPath =
                Path.Combine(
                    Path.GetTempPath(),
                    $"WAMP-DS-FinalCleanup-{Guid.NewGuid():N}.ps1");

            string escapedInstallationPath =
                _installationPath.Replace(
                    "'",
                    "''");

            string script =
                "Start-Sleep -Seconds 2" +
                Environment.NewLine +
                Environment.NewLine +
                $"$installationPath = '{escapedInstallationPath}'" +
                Environment.NewLine +
                Environment.NewLine +
                "if (Test-Path $installationPath) {" +
                Environment.NewLine +
                "    Remove-Item $installationPath -Recurse -Force -ErrorAction SilentlyContinue" +
                Environment.NewLine +
                "}" +
                Environment.NewLine +
                Environment.NewLine +
                "Remove-Item $PSCommandPath -Force -ErrorAction SilentlyContinue" +
                Environment.NewLine;

            File.WriteAllText(
                scriptPath,
                script);

            Process.Start(
                new ProcessStartInfo
                {
                    FileName =
                        "powershell.exe",

                    Arguments =
                        $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",

                    UseShellExecute =
                        false,

                    CreateNoWindow =
                        true,

                    WindowStyle =
                        ProcessWindowStyle.Hidden
                });

            Close();
        }

        private void WriteOutput(
            string message)
        {
            OutputTextBox.AppendText(
                message +
                Environment.NewLine);

            OutputTextBox.ScrollToEnd();
        }
    }
}