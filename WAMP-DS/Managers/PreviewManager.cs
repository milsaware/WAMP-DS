using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public enum PhpStatus
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Failed
    }

    public class PreviewManager
    {
        private Process? phpProcess;

        public event EventHandler? StatusChanged;

        public PhpStatus Status { get; private set; } =
            PhpStatus.Stopped;

        public int Port { get; } = 8000;

        public string Version { get; } = "8.5.8";

        public bool IsRunning =>
            phpProcess != null &&
            !phpProcess.HasExited;

        public async Task<bool> StartPhpServer(
            string projectPath)
        {
            if (IsRunning)
                return true;

            string phpPath =
                GetPhpPath();

            if (!File.Exists(phpPath))
            {
                SetStatus(
                    PhpStatus.Failed
                );

                MessageBox.Show(
                    $"Unable to find the PHP runtime.\n\n{phpPath}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return false;
            }

            SetStatus(
                PhpStatus.Starting
            );

            phpProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = phpPath,
                    Arguments =
                        $"-S 127.0.0.1:{Port} -t \"{projectPath}\"",
                    WorkingDirectory = projectPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            phpProcess.Exited += PhpProcess_Exited;

            try
            {
                phpProcess.Start();

                await Task.Delay(
                    100
                );

                if (phpProcess.HasExited)
                {
                    SetStatus(
                        PhpStatus.Failed
                    );

                    phpProcess.Dispose();
                    phpProcess = null;

                    return false;
                }

                SetStatus(
                    PhpStatus.Running
                );

                return true;
            }
            catch (Exception ex)
            {
                SetStatus(
                    PhpStatus.Failed
                );

                MessageBox.Show(
                    $"Unable to start the PHP server.\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                phpProcess?.Dispose();
                phpProcess = null;

                return false;
            }
        }

        public string GetPreviewUrl(
    string projectPath,
    string phpFilePath)
        {
            string settingsPath =
                Path.Combine(
                    projectPath,
                    "settings.wampds"
                );


            Debug.WriteLine(
                $"Looking for settings: {settingsPath}"
            );


            Debug.WriteLine(
                $"Settings exists: {File.Exists(settingsPath)}"
            );


            ProjectSettings? settings =
                LoadProjectSettings(projectPath);


            if (settings != null)
            {
                Debug.WriteLine(
                    $"Project domain: {settings.Domain}"
                );

                Debug.WriteLine(
                    $"Project SSL: {settings.Ssl}"
                );
            }
            else
            {
                Debug.WriteLine(
                    "No project settings loaded."
                );
            }


            string relativePath =
                Path.GetRelativePath(
                    projectPath,
                    phpFilePath
                );


            relativePath =
                relativePath.Replace(
                    '\\',
                    '/'
                );


            if (settings != null &&
                !string.IsNullOrWhiteSpace(settings.Domain))
            {
                string protocol =
                    settings.Ssl
                        ? "https"
                        : "http";


                return
                    $"{protocol}://{settings.Domain}/{relativePath}";
            }


            return
                $"http://127.0.0.1:{Port}/{relativePath}";
        }

        public string? SelectPreviewEntryPoint(
            string projectPath)
        {
            string htmlIndexPath =
                Path.Combine(
                    projectPath,
                    "index.html"
                );

            if (File.Exists(htmlIndexPath))
                return htmlIndexPath;

            string phpIndexPath =
                Path.Combine(
                    projectPath,
                    "index.php"
                );

            if (File.Exists(phpIndexPath))
                return phpIndexPath;

            MessageBox.Show(
                "WAMP-DS could not find an index.html or index.php file in the project root.\n\n" +
                "Please select the file you want to use as the preview entry point.",
                "Select Preview Entry Point",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            OpenFileDialog dialog = new()
            {
                Title = "Select Preview Entry Point",
                Filter =
                    "Web Files (*.html;*.htm;*.php)|*.html;*.htm;*.php|" +
                    "HTML Files (*.html;*.htm)|*.html;*.htm|" +
                    "PHP Files (*.php)|*.php",
                InitialDirectory = projectPath,
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
                return null;

            string selectedPath =
                dialog.FileName;

            string relativePath =
                Path.GetRelativePath(
                    projectPath,
                    selectedPath
                );

            if (relativePath.StartsWith(".."))
            {
                MessageBox.Show(
                    "The selected file must be inside the current project.",
                    "Select Preview Entry Point",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return null;
            }

            return selectedPath;
        }

        public void StopPhpServer()
        {
            if (phpProcess == null)
            {
                SetStatus(
                    PhpStatus.Stopped
                );

                return;
            }

            SetStatus(
                PhpStatus.Stopping
            );

            try
            {
                phpProcess.Exited -= PhpProcess_Exited;

                if (!phpProcess.HasExited)
                {
                    phpProcess.Kill();
                    phpProcess.WaitForExit();
                }
            }
            catch
            {
            }
            finally
            {
                phpProcess.Dispose();
                phpProcess = null;

                SetStatus(
                    PhpStatus.Stopped
                );
            }
        }

        private void PhpProcess_Exited(
            object? sender,
            EventArgs e)
        {
            if (Status == PhpStatus.Stopping)
                return;

            phpProcess?.Dispose();
            phpProcess = null;

            SetStatus(
                PhpStatus.Stopped
            );
        }

        private void SetStatus(
            PhpStatus status)
        {
            Status = status;

            StatusChanged?.Invoke(
                this,
                EventArgs.Empty
            );
        }

        private static string GetPhpPath()
        {
            return Path.Combine(
                AppContext.BaseDirectory,
                "runtimes",
                "php",
                "8.5.8",
                "php.exe"
            );
        }

        public ProjectSettings? LoadProjectSettings(string projectPath)
        {
            string settingsFile =
                Path.Combine(
                    projectPath,
                    "settings.wampds"
                );

            if (!File.Exists(settingsFile))
                return null;


            try
            {
                string json =
                    File.ReadAllText(
                        settingsFile
                    );

                return JsonSerializer.Deserialize<ProjectSettings>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );
            }
            catch
            {
                return null;
            }
        }
    }
}