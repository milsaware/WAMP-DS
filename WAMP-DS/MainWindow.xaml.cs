using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WAMP_DS.Core;
using WAMP_DS.Managers;
using WAMP_DS.Models;
using WAMP_DS.Views;

namespace WAMP_DS
{
    public partial class MainWindow : Window
    {
        private readonly ProjectManager projectManager = new();
        private readonly EditorManager editorManager;
        private PreviewWindow? previewWindow;
        private readonly PreviewManager phpPreviewManager = new();
        private readonly MySQLManager mysqlManager;
        private readonly MySQLSettingsManager mysqlSettingsManager;
        private readonly ApacheManager apacheManager;
        private readonly InstallationPaths installationPaths = new();
        private readonly InstallationValidator installationValidator;
        private readonly ProjectCreationManager projectCreationManager;
        private ProjectCreationWindow? projectCreationWindow;
        private readonly OpenSearchManager openSearchManager = new();
        private readonly DeveloperToolsManager developerToolsManager;
        private readonly MagentoManager magentoManager;
        private bool _isClosing;

        private bool webMessageHooked = false;

        public MainWindow()
        {
            InitializeComponent();

            LivePreviewBrowser.CoreWebView2InitializationCompleted += LivePreviewBrowser_CoreWebView2InitializationCompleted;

            installationValidator = new InstallationValidator(
                installationPaths
            );

            editorManager = new EditorManager(
                CodeEditor
            );

            EditorTabs.DocumentSelected += EditorTabs_DocumentSelected;

            EditorTabs.DocumentCloseRequested += EditorTabs_DocumentCloseRequested;

            EditorTabs.DocumentReordered += EditorTabs_DocumentReordered;

            UpdateWorkspaceVisibility();

            mysqlManager = new MySQLManager();

            mysqlManager.StatusChanged += MySQLManager_StatusChanged;

            mysqlSettingsManager = new MySQLSettingsManager();

            apacheManager = new ApacheManager(
                installationPaths
            );

            magentoManager = new MagentoManager(
                installationPaths
            );

            DatabaseManager databaseManager = new DatabaseManager(
                mysqlSettingsManager
            );

            projectCreationManager = new ProjectCreationManager(
                projectManager,
                apacheManager,
                databaseManager,
                mysqlSettingsManager,
                installationPaths
            );

            developerToolsManager = new DeveloperToolsManager();

            projectCreationManager.ProgressChanged += ProjectCreationManager_ProgressChanged;

            apacheManager.StatusChanged += ApacheManager_StatusChanged;

            openSearchManager.StatusChanged += OpenSearchManager_StatusChanged;

            Loaded += MainWindow_Loaded;

            Closing += MainWindow_Closing;

            LivePreviewBrowser.SizeChanged += LivePreviewBrowser_SizeChanged;
        }

        private void LivePreviewBrowser_CoreWebView2InitializationCompleted(
            object? sender,            Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e
        )
        {
            if (!e.IsSuccess)
                return;
            LivePreviewBrowser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
    @"
    (() => {

        const originalLog = console.log;
        const originalWarn = console.warn;
        const originalError = console.error;


        console.log = function(...args)
        {
            window.chrome.webview.postMessage(
                '[LOG] ' + args.join(' ')
            );

            originalLog.apply(console,args);
        };


        console.warn = function(...args)
        {
            window.chrome.webview.postMessage(
                '[WARN] ' + args.join(' ')
            );

            originalWarn.apply(console,args);
        };


        console.error = function(...args)
        {
            window.chrome.webview.postMessage(
                '[ERROR] ' + args.join(' ')
            );

            originalError.apply(console,args);
        };


        window.onerror = function(message, source, lineno, colno, error)
        {
            window.chrome.webview.postMessage(
                '[ERROR] ' +
                message +
                ' (' +
                source +
                ':' +
                lineno +
                ':' +
                colno +
                ')'
            );
        };


        window.addEventListener(
            'unhandledrejection',
            function(event)
            {
                window.chrome.webview.postMessage(
                    '[ERROR] Unhandled Promise Rejection: ' +
                    event.reason
                );
            }
        );


    })();
    "
    );
        }

        private void LivePreviewColumn_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            UpdatePreviewDimensions();
        }

        private void UpdatePreviewDimensions()
        {
            int width = (int)LivePreviewBrowser.ActualWidth;

            int height = (int)LivePreviewBrowser.ActualHeight;


            PreviewDimensionsText.Text = $"{width} x {height}";
        }

        private void LivePreviewBrowser_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            PreviewDimensionsText.Text =
                $"{(int)LivePreviewBrowser.ActualWidth} x {(int)LivePreviewBrowser.ActualHeight}";
        }

        private void OpenSearchManager_StatusChanged(
        object? sender,
        EventArgs e)
        {
            Dispatcher.Invoke(
                UpdateOpenSearchStatus
            );
        }

        private void UpdateOpenSearchStatus()
        {
            switch (openSearchManager.Status)
            {
                case OpenSearchStatus.Starting:
                    OpenSearchStatusText.Text = "Starting";

                    OpenSearchStatusIndicator.Foreground =
                        System.Windows.Media.Brushes.Gold;

                    OpenSearchControlButton.Content = "Starting...";

                    OpenSearchControlButton.IsEnabled = false;
                break;

                case OpenSearchStatus.Running:
                    OpenSearchStatusText.Text = "Running";

                    OpenSearchStatusIndicator.Foreground = System.Windows.Media.Brushes.LightGreen;

                    OpenSearchControlButton.Content = "Stop";

                    OpenSearchControlButton.IsEnabled = true;
                break;

                case OpenSearchStatus.Failed:
                    OpenSearchStatusText.Text = "Failed";

                    OpenSearchStatusIndicator.Foreground = System.Windows.Media.Brushes.IndianRed;

                    OpenSearchControlButton.Content = "Start";

                    OpenSearchControlButton.IsEnabled = true;
                break;

                default:
                    OpenSearchStatusText.Text = "Stopped";

                    OpenSearchStatusIndicator.Foreground = System.Windows.Media.Brushes.Gray;

                    OpenSearchControlButton.Content = "Start";

                    OpenSearchControlButton.IsEnabled = true;
                break;
            }

            OpenSearchPortText.Text = $"Port: {openSearchManager.Port}";

            OpenSearchVersionText.Text = $"Version: {openSearchManager.Version}";
        }

        private void ProjectCreationManager_ProgressChanged(
            object? sender,
            string message)
        {
            Dispatcher.Invoke(() =>
            {
                projectCreationWindow?.UpdateStatus(message);
            });
        }

        private void ApacheManager_StatusChanged(
            object? sender,
            EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateApacheStatus();
                UpdateStatusBar();
            });
        }

        private void UpdateApacheStatus()
        {
            switch (apacheManager.Status)
            {
                case ApacheStatus.Starting:
                    ApacheStatusText.Text = "Starting";
                    ApacheStatusIndicator.Text = "●";
                    ApacheStatusIndicator.Foreground = System.Windows.Media.Brushes.Gold;
                    ApacheControlButton.Content = "Starting...";
                    ApacheControlButton.IsEnabled = false;
                break;

                case ApacheStatus.Running:
                    ApacheStatusText.Text = "Running";
                    ApacheStatusIndicator.Text = "●";
                    ApacheStatusIndicator.Foreground = System.Windows.Media.Brushes.LightGreen;
                    ApacheControlButton.Content = "Stop";
                    ApacheControlButton.IsEnabled = true;
                break;

                case ApacheStatus.Stopping:
                    ApacheStatusText.Text = "Stopping";
                    ApacheStatusIndicator.Text = "●";
                    ApacheStatusIndicator.Foreground = System.Windows.Media.Brushes.Gold;
                    ApacheControlButton.Content = "Stopping...";
                    ApacheControlButton.IsEnabled = false;
                break;

                case ApacheStatus.Failed:
                    ApacheStatusText.Text = "Failed";
                    ApacheStatusIndicator.Text = "●";
                    ApacheStatusIndicator.Foreground = System.Windows.Media.Brushes.IndianRed;
                    ApacheControlButton.Content = "Start";
                    ApacheControlButton.IsEnabled = true;
                break;

                default:
                    ApacheStatusText.Text = "Stopped";
                    ApacheStatusIndicator.Text = "●";
                    ApacheStatusIndicator.Foreground = System.Windows.Media.Brushes.Gray;
                    ApacheControlButton.Content = "Start";
                    ApacheControlButton.IsEnabled = true;
                break;
            }

            ApachePortText.Text = $"Port: {apacheManager.Port}";

            ApacheVersionText.Text = $"Version: {apacheManager.Version}";

            UpdateSslCard();

            UpdateStatusBar();
        }

        private void UpdateSslCard()
        {
            bool sslEnabled = apacheManager.IsHttpsEnabled();

            SslPortText.Text = $"Port: {apacheManager.HttpsPort}";

            if (sslEnabled)
            {
                SslStatusText.Text = "Enabled";

                SslStatusIndicator.Text = "●";

                SslStatusIndicator.Foreground =
                    System.Windows.Media.Brushes.LightGreen;

                SslControlButton.Content = "Disable";

                SslControlButton.IsEnabled = true;

                if (File.Exists(apacheManager.ServerCertificate))
                {
                    SslCertificateText.Text = "Certificate: Configured";
                }
                else
                {
                    SslCertificateText.Text = "Certificate: Missing";
                }
            }
            else
            {
                SslStatusText.Text = "Disabled";

                SslStatusIndicator.Text = "●";

                SslStatusIndicator.Foreground =
                    System.Windows.Media.Brushes.Gray;

                SslControlButton.Content = "Enable";

                SslControlButton.IsEnabled = true;

                SslCertificateText.Text = "Certificate: Not configured";
            }
        }

        private void UpdateStatusBar()
        {
            bool mysqlRunning =
                mysqlManager.Status == MySQLStatus.Running;

            bool apacheRunning =
                apacheManager.Status == ApacheStatus.Running;

            bool openSearchRunning =
                openSearchManager.Status == OpenSearchStatus.Running;

            bool mysqlStarting =
                mysqlManager.Status == MySQLStatus.Starting ||
                mysqlManager.Status == MySQLStatus.Stopping;

            bool apacheStarting =
                apacheManager.Status == ApacheStatus.Starting ||
                apacheManager.Status == ApacheStatus.Stopping;

            // Services transitioning
            if (mysqlStarting || apacheStarting)
            {
                StatusBar.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            180,
                            120,
                            0
                        )
                    );

                StatusBarText.Text = "Starting services...";

                SslStatusIcon.Text = "⏳";

                return;
            }

            // One or both services offline
            if (!mysqlRunning || !apacheRunning)
            {
                StatusBar.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            220,
                            53,
                            69
                        )
                    );

                SslStatusIcon.Text = "⚠";

                if (!apacheRunning && !mysqlRunning)
                {
                    StatusBarText.Text = "Apache + MySQL Offline";
                }
                else if (!apacheRunning)
                {
                    StatusBarText.Text = "Apache Offline";
                }
                else
                {
                    StatusBarText.Text = "MySQL Offline";
                }

                return;
            }

            // Both running - SSL decides blue/green
            bool sslEnabled = apacheManager.IsHttpsEnabled();

            if (sslEnabled)
            {
                StatusBar.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            40,
                            167,
                            69
                        )
                    );

                SslStatusIcon.Text =
                    "🔒";

                if (openSearchRunning)
                {
                    StatusBarText.Text = "HTTPS • Apache + MySQL + OpenSearch Online";
                }
                else
                {
                    StatusBarText.Text = "HTTPS • Apache + MySQL Online";
                }
            }
            else
            {
                StatusBar.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            0,
                            122,
                            204
                        )
                    );

                SslStatusIcon.Text = "🌐";

                if (openSearchRunning)
                {
                    StatusBarText.Text =
                        "HTTP • Apache + MySQL + OpenSearch Online";
                }
                else
                {
                    StatusBarText.Text = "HTTP • Apache + MySQL Online";
                }
            }
        }

        private async void MainWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await ShowNoProjectLoaded();

            if (!installationValidator.IsValid())
            {
                MessageBox.Show(
                    "WAMP-DS installation is incomplete.",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
                return;
            }

            // Make SERVER the default selected tab.
            ShowBottomPanel(
                OutputPanel,
                OutputTabButton
            );

            // Update the server cards before starting services.
            UpdateMySQLStatus();
            UpdateApacheStatus();
            UpdateOpenSearchStatus();
            UpdateSslCard();

            try
            {
                await mysqlManager.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to start MySQL.\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            try
            {
                string? projectPath = projectManager.CurrentProjectPath;

                await apacheManager.StartAsync(
                    projectPath
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to start Apache.\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            try
            {
                await openSearchManager.StartAsync(
                    installationPaths.OpenSearchPath
                );

                UpdateOpenSearchStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to start OpenSearch.\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            // Refresh SSL state after Apache startup.
            UpdateSslCard();
            UpdateStatusBar();
        }

        private async void SslControlButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                bool sslEnabled = apacheManager.IsHttpsEnabled();

                SslControlButton.IsEnabled = false;

                // Stop Apache before changing SSL configuration
                if (apacheManager.Status == ApacheStatus.Running)
                {
                    apacheManager.Stop();

                    await Task.Delay(500);
                }

                if (sslEnabled)
                {
                    apacheManager.DisableHttps();
                }
                else
                {
                    apacheManager.EnableHttps();
                }

                // Restart Apache with new configuration
                await apacheManager.StartAsync();

                UpdateApacheStatus();
                UpdateSslCard();
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to change the SSL state.\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                UpdateSslCard();
                UpdateStatusBar();
            }
            finally
            {
                SslControlButton.IsEnabled = true;
            }
        }

        private void MySQLManager_StatusChanged(
            object? sender,
            EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateMySQLStatus();
                UpdateStatusBar();
            });
        }

        private void UpdateMySQLStatus()
        {
            switch (mysqlManager.Status)
            {
                case MySQLStatus.Starting:
                    MySQLStatusText.Text = "Starting";

                    MySQLStatusIndicator.Text = "●";

                    MySQLStatusIndicator.Foreground =
                        System.Windows.Media.Brushes.Gold;

                    MySQLControlButton.Content = "Starting...";

                    MySQLControlButton.IsEnabled = false;
                break;

                case MySQLStatus.Running:
                    MySQLStatusText.Text = "Running";

                    MySQLStatusIndicator.Text = "●";

                    MySQLStatusIndicator.Foreground =
                        System.Windows.Media.Brushes.LightGreen;

                    MySQLControlButton.Content = "Stop";

                    MySQLControlButton.IsEnabled = true;
                break;


                case MySQLStatus.Stopping:
                    MySQLStatusText.Text = "Stopping";

                    MySQLStatusIndicator.Text = "●";

                    MySQLStatusIndicator.Foreground =
                        System.Windows.Media.Brushes.Gold;

                    MySQLControlButton.Content = "Stopping...";

                    MySQLControlButton.IsEnabled = false;
                break;

                case MySQLStatus.Failed:
                    MySQLStatusText.Text = "Failed";

                    MySQLStatusIndicator.Text = "●";

                    MySQLStatusIndicator.Foreground =
                        System.Windows.Media.Brushes.IndianRed;

                    MySQLControlButton.Content = "Start";

                    MySQLControlButton.IsEnabled = true;
                break;

                default:
                    MySQLStatusText.Text = "Stopped";

                    MySQLStatusIndicator.Text = "●";

                    MySQLStatusIndicator.Foreground =
                        System.Windows.Media.Brushes.Gray;

                    MySQLControlButton.Content = "Start";

                    MySQLControlButton.IsEnabled = true;
                break;
            }

            MySQLPortText.Text =
                $"Port: {mysqlManager.Port}";

            MySQLVersionText.Text =
                $"Version: {mysqlManager.Version}";
        }

        private async void MainWindow_Closing(
            object? sender,
            System.ComponentModel.CancelEventArgs e)
        {
            if (_isClosing)
                return;

            e.Cancel = true;
            _isClosing = true;

            Debug.WriteLine("Closing started");

            phpPreviewManager.StopPhpServer();
            Debug.WriteLine("PHP stopped");

            await Task.Run(() =>
            {
                try
                {
                    apacheManager.Stop();
                    Debug.WriteLine("Apache stopped");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"Apache shutdown failed: {ex.Message}"
                    );
                }

                try
                {
                    mysqlManager.Stop();
                    Debug.WriteLine("MySQL stopped");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"MySQL shutdown failed: {ex.Message}"
                    );
                }

                try
                {
                    openSearchManager.Kill();
                    Debug.WriteLine("OpenSearch killed");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"OpenSearch shutdown failed: {ex.Message}"
                    );
                }
            });

            Application.Current.Shutdown();
        }

        private void SettingsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SettingsWindow settingsWindow =
                new SettingsWindow(
                    apacheManager,
                    mysqlManager,
                    mysqlSettingsManager
                );

            settingsWindow.Show();
        }

        private async void MySQLControlButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (mysqlManager.Status ==
                    MySQLStatus.Running)
                {
                    mysqlManager.Stop();

                    return;
                }

                if (mysqlManager.Status ==
                        MySQLStatus.Stopped ||
                    mysqlManager.Status ==
                        MySQLStatus.Failed)
                {
                    await mysqlManager.StartAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to change the MySQL server state.\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void ApacheControlButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (apacheManager.Status ==
                    ApacheStatus.Running)
                {
                    apacheManager.Stop();

                    return;
                }

                if (apacheManager.Status ==
                        ApacheStatus.Stopped ||
                    apacheManager.Status ==
                        ApacheStatus.Failed)
                {
                    await apacheManager.StartAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to change the Apache server state.\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void PhpMyAdminButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            PhpMyAdminWindow phpMyAdminWindow = new PhpMyAdminWindow()
            {
                Owner = this
            };

            phpMyAdminWindow.Show();
        }

        private void EditorTabs_DocumentReordered(
            OpenDocument document,
            int targetIndex)
        {
            int currentIndex =
                editorManager.OpenDocuments.IndexOf(
                    document
                );

            if (currentIndex < 0)
                return;

            if (targetIndex > currentIndex)
                targetIndex--;

            if (targetIndex < 0)
                targetIndex = 0;

            if (targetIndex >=
                editorManager.OpenDocuments.Count)
            {
                targetIndex =
                    editorManager.OpenDocuments.Count - 1;
            }

            if (currentIndex == targetIndex)
                return;

            editorManager.OpenDocuments.Move(
                currentIndex,
                targetIndex
            );

            EditorTabs.SetDocuments(
                editorManager.OpenDocuments
            );
        }

        private void OutputTabButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowBottomPanel(
                OutputPanel,
                OutputTabButton
            );
        }

        private void TerminalTabButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowBottomPanel(
                TerminalPanel,
                TerminalTabButton
            );
        }

        private void ServerTabButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowBottomPanel(
                ServerPanel,
                ServerTabButton
            );
        }

        private async void PreviewButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (previewWindow != null)
            {
                previewWindow.Close();
                return;
            }

            string? projectPath =
                projectManager.CurrentProjectPath;

            if (string.IsNullOrEmpty(projectPath))
            {
                MessageBox.Show(
                    "There is no open project to preview.",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            string? previewUrl = null;

            ProjectSettings? settings =
                phpPreviewManager.LoadProjectSettings(
                    projectPath
                );

            if (settings != null &&
                !string.IsNullOrWhiteSpace(settings.Domain))
            {
                Debug.WriteLine(
                    $"WAMP-DS Settings found. Domain: {settings.Domain}, SSL: {settings.Ssl}"
                );

                string protocol =
                    settings.Ssl
                        ? "https"
                        : "http";

                previewUrl =
                    $"{protocol}://{settings.Domain}/";
            }
            else
            {
                string? previewFilePath =
                    phpPreviewManager.SelectPreviewEntryPoint(
                        projectPath
                    );

                if (string.IsNullOrEmpty(previewFilePath))
                    return;

                bool started =
                    await phpPreviewManager.StartPhpServer(
                        projectPath
                    );

                if (!started)
                    return;

                previewUrl =
                    phpPreviewManager.GetPreviewUrl(
                        projectPath,
                        previewFilePath
                    );
            }

            OpenDocument? document =
                editorManager.ActiveDocument;

            if (document != null &&
                document.IsModified)
            {
                bool saved =
                    editorManager.SaveDocument(
                        document,
                        out string? errorMessage
                    );

                if (!saved)
                {
                    MessageBox.Show(
                        $"Unable to save the file.\n\n{errorMessage}",
                        "WAMP-DS",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    return;
                }
            }

            previewWindow = new PreviewWindow(previewUrl)
            {
                Owner = this
            };

            previewWindow.Closed += PreviewWindow_Closed;

            previewWindow.Show();

            PreviewButton.Content = "■";
            PreviewButton.ToolTip = "Stop Preview";
        }

        private void PreviewWindow_Closed(
            object? sender,
            EventArgs e)
        {
            previewWindow = null;

            PreviewButton.Content = "▶";

            PreviewButton.ToolTip = "Preview";
        }

        private void ShowBottomPanel(
            UIElement panel,
            Button activeButton)
        {
            OutputPanel.Visibility = Visibility.Collapsed;

            TerminalPanel.Visibility = Visibility.Collapsed;

            ServerPanel.Visibility = Visibility.Collapsed;

            OutputTabBorder.Background =
                System.Windows.Media.Brushes.Transparent;

            TerminalTabBorder.Background =
                System.Windows.Media.Brushes.Transparent;

            ServerTabBorder.Background =
                System.Windows.Media.Brushes.Transparent;

            OutputTabBorder.BorderBrush =
                System.Windows.Media.Brushes.Transparent;

            TerminalTabBorder.BorderBrush =
                System.Windows.Media.Brushes.Transparent;

            ServerTabBorder.BorderBrush =
                System.Windows.Media.Brushes.Transparent;

            OutputTabButton.Foreground =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(
                        204,
                        204,
                        204
                    )
                );

            TerminalTabButton.Foreground =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(
                        204,
                        204,
                        204
                    )
                );

            ServerTabButton.Foreground =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(
                        204,
                        204,
                        204
                    )
                );

            panel.Visibility = Visibility.Visible;

            activeButton.Foreground =
                System.Windows.Media.Brushes.White;

            if (activeButton ==
                OutputTabButton)
            {
                OutputTabBorder.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            37,
                            37,
                            38
                        )
                    );

                OutputTabBorder.BorderBrush =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            0,
                            122,
                            204
                        )
                    );
            }
            else if (activeButton == TerminalTabButton)
            {
                TerminalTabBorder.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            37,
                            37,
                            38
                        )
                    );

                TerminalTabBorder.BorderBrush =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            0,
                            122,
                            204
                        )
                    );
            }
            else if (activeButton == ServerTabButton)
            {
                ServerTabBorder.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            37,
                            37,
                            38
                        )
                    );

                ServerTabBorder.BorderBrush = new System.Windows.Media.SolidColorBrush
                (
                    System.Windows.Media.Color.FromRgb(
                        0,
                        122,
                        204
                    )
                );
            }
        }

        private void UpdateWorkspaceVisibility()
        {
            bool hasOpenDocuments =
                editorManager.OpenDocuments.Count > 0;

            WelcomePanel.Visibility =
                hasOpenDocuments
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            EditorWorkspace.Visibility =
                hasOpenDocuments
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void EditorTabs_DocumentSelected(
            OpenDocument document)
        {
            editorManager.SetActiveDocument(
                document
            );

            EditorTabs.SetDocuments(
                editorManager.OpenDocuments
            );
        }

        private void EditorTabs_DocumentCloseRequested(
            OpenDocument document)
        {
            if (!CanCloseDocument(
                document))
                return;

            editorManager.CloseDocument(
                document
            );

            EditorTabs.SetDocuments(
                editorManager.OpenDocuments
            );

            UpdateWorkspaceVisibility();
        }

        private bool CanCloseDocument(
            OpenDocument document)
        {
            if (!document.IsModified)
                return true;

            MessageBoxResult result = MessageBox.Show(
                $"Do you want to save changes to {document.FileName}?",
                "WAMP-DS",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Cancel)
                return false;

            if (result == MessageBoxResult.No)
                return true;

            bool saved = editorManager.SaveDocument(
                document,
                out string? errorMessage
            );

            if (saved)
                return true;

            MessageBox.Show(
                $"Unable to save {document.FileName}.\n\n{errorMessage}",
                "WAMP-DS",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            return false;
        }

        private void SaveAsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            bool saved = editorManager.SaveActiveDocumentAs(
                out string? errorMessage
            );

            if (!saved)
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show(
                        $"Unable to save the file.\n\n{errorMessage}",
                        "WAMP-DS",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }

                return;
            }

            EditorTabs.SetDocuments(
                editorManager.OpenDocuments
            );

            RefreshProjectTree();
        }

        private async void OpenProjectButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Title = "Select a project folder",

                CheckFileExists = false,

                CheckPathExists = true,

                FileName = "Select Folder"
            };

            if (dialog.ShowDialog() != true)
                return;

            string? projectPath = Path.GetDirectoryName(
                dialog.FileName
            );

            if (string.IsNullOrEmpty(projectPath))
                return;

            projectManager.OpenProject(
                projectPath
            );

            RefreshProjectTree();

            await LoadDockedPreview();
        }

        private void ProjectTree_Expanded(
            object sender,
            RoutedEventArgs e)
        {
            if (e.OriginalSource is not TreeViewItem treeItem)
                return;

            if (treeItem.Tag is not ProjectItem projectItem)
                return;

            projectManager.LoadChildren(
                projectItem
            );

            treeItem.Items.Clear();

            foreach (ProjectItem child in projectItem.Children)
            {
                treeItem.Items.Add(
                    CreateTreeViewItem(child)
                );
            }
        }

        private void RefreshProjectTree()
        {
            if (!projectManager.IsProjectOpen)
                return;

            projectManager.LoadProjectItems();

            ProjectTreeView.Items.Clear();

            TreeViewItem projectTreeItem = new()
            {
                Tag = projectManager.CurrentProjectPath,

                Foreground = new System.Windows.Media.SolidColorBrush
                (
                    System.Windows.Media.Color.FromRgb(
                        204,
                        204,
                        204
                    )
                )
            };

            StackPanel headerPanel = new()
            {
                Orientation = Orientation.Horizontal
            };

            TextBlock icon = new()
            {
                Text = "📁",
                FontSize = 14,
                Margin = new Thickness(0, 0, 6, 0)
            };

            TextBlock name = new()
            {
                Text = projectManager.CurrentProjectName,

                VerticalAlignment = VerticalAlignment.Center
            };

            headerPanel.Children.Add(icon);
            headerPanel.Children.Add(name);

            projectTreeItem.Header = headerPanel;

            projectTreeItem.ContextMenu = CreateRootExplorerContextMenu();

            foreach (ProjectItem item in projectManager.ProjectItems)
            {
                projectTreeItem.Items.Add(
                    CreateTreeViewItem(
                        item
                    )
                );
            }

            ProjectTreeView.Items.Add(projectTreeItem);

            projectTreeItem.IsExpanded = true;
        }

        private TreeViewItem CreateTreeViewItem(
            ProjectItem item)
        {
            TreeViewItem treeItem =new()
            {
                Tag = item,

                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(
                        204,
                        204,
                        204
                    )
                )
            };

            StackPanel headerPanel = new()
            {
                Orientation = Orientation.Horizontal
            };

            TextBlock icon = new()
            {
                Text = item.IsDirectory? "📁" : "📄",

                FontSize = 14,

                Margin = new Thickness(0, 0, 6, 0)
            };

            TextBlock name = new()
            {
                Text = item.Name,

                VerticalAlignment = VerticalAlignment.Center
            };

            headerPanel.Children.Add(icon);
            headerPanel.Children.Add(name);

            treeItem.Header = headerPanel;

            if (item.IsDirectory)
            {
                foreach (ProjectItem childItem in item.Children)
                {
                    treeItem.Items.Add(
                        CreateTreeViewItem(
                            childItem
                        )
                    );
                }
            }

            treeItem.ContextMenu = CreateExplorerContextMenu(item);

            return treeItem;
        }

        private ContextMenu CreateExplorerContextMenu(ProjectItem item)
        {
            ContextMenu menu = new();

            if (item.IsDirectory)
            {
                MenuItem newFile = new()
                {
                    Header = "📄 New File"
                };

                newFile.Click += (s, e) =>
                {
                    CreateNewExplorerFile(
                        item.FullPath
                    );
                };

                MenuItem newFolder = new()
                {
                    Header = "📁 New Folder"
                };

                newFolder.Click += (s, e) =>
                {
                    CreateNewExplorerFolder(
                        item.FullPath
                    );
                };

                menu.Items.Add(newFile);
                menu.Items.Add(newFolder);
            }
            else
            {
                MenuItem open = new()
                {
                    Header = "✏ Open"
                };

                open.Click += (s, e) =>
                {
                    editorManager.OpenFile(item.FullPath);

                    EditorTabs.SetDocuments(
                        editorManager.OpenDocuments
                    );

                    UpdateWorkspaceVisibility();
                };

                MenuItem delete = new()
                {
                    Header = "🗑 Delete"
                };

                delete.Click += (s, e) =>
                {
                    if (MessageBox.Show(
                        $"Delete {item.Name}?",
                        "WAMP-DS",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning)
                        == MessageBoxResult.Yes)
                    {
                        File.Delete(item.FullPath);
                        RefreshProjectTree();
                    }
                };

                menu.Items.Add(open);
                menu.Items.Add(delete);
            }

            return menu;
        }

        private ContextMenu CreateRootExplorerContextMenu()
        {
            ContextMenu menu = new();

            MenuItem newFile = new()
            {
                Header = "📄 New File"
            };

            newFile.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(projectManager.CurrentProjectPath))
                    return;

                CreateNewExplorerFile(
                    projectManager.CurrentProjectPath
                );
            };

            MenuItem newFolder = new()
            {
                Header = "📁 New Folder"
            };

            newFolder.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(projectManager.CurrentProjectPath))
                    return;

                CreateNewExplorerFolder(
                    projectManager.CurrentProjectPath
                );
            };

            menu.Items.Add(newFile);
            menu.Items.Add(newFolder);

            return menu;
        }

        private void CreateNewExplorerFile(string folderPath)
        {
            NewFileDialog dialog = new NewFileDialog()
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
                return;

            string extension = dialog.SelectedExtension;

            string fileName = $"NewFile{extension}";

            string filePath = Path.Combine(
                folderPath,
                fileName
            );

            int counter = 1;

            while (File.Exists(filePath))
            {
                fileName = $"NewFile{counter}{extension}";

                filePath = Path.Combine(
                    folderPath,
                    fileName
                );

                counter++;
            }

            File.WriteAllText(
                filePath,
                dialog.SelectedTemplate ?? string.Empty
            );

            RefreshProjectTree();
        }

        private void CreateNewExplorerFolder(string folderPath)
        {
            NewFolderDialog dialog = new NewFolderDialog()
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
                return;

            string folderName = dialog.FolderName;

            if (string.IsNullOrWhiteSpace(folderName))
                return;

            string newFolderPath = Path.Combine(
                folderPath,
                folderName
            );

            if (Directory.Exists(newFolderPath))
            {
                MessageBox.Show(
                    "A folder with that name already exists.",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            Directory.CreateDirectory(
                newFolderPath
            );

            RefreshProjectTree();
        }

        private void ProjectTreeView_MouseDoubleClick(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject? source = e.OriginalSource as DependencyObject;

            TreeViewItem? clickedItem = FindParent<TreeViewItem>(
                source
            );

            if (clickedItem == null)
                return;

            if (clickedItem.Tag is not ProjectItem projectItem)
                return;

            string selectedPath = projectItem.FullPath;

            if (Directory.Exists(selectedPath))
                return;

            if (!File.Exists(selectedPath))
                return;

            editorManager.OpenFile(
                selectedPath
            );

            EditorTabs.SetDocuments(
                editorManager.OpenDocuments
            );

            UpdateWorkspaceVisibility();
        }

        private static T? FindParent<T>(
            DependencyObject? child)
            where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;

                child = System.Windows.Media.VisualTreeHelper.GetParent(
                    child
                );
            }

            return null;
        }

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            bool saved = editorManager.SaveActiveDocument();

            if (saved)
            {
                previewWindow?.RefreshPreview();

                RefreshDockedPreview();
            }
        }

        private void NewFileButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            NewFileDialog dialog = new NewFileDialog()
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
                return;

            editorManager.CreateNewDocument(
                dialog.SelectedExtension,
                dialog.SelectedTemplate
            );

            EditorTabs.SetDocuments(
                editorManager.OpenDocuments
            );

            UpdateWorkspaceVisibility();
        }

        private async void NewProjectButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await OpenNewProject();
        }

        private async Task OpenNewProject()
        {
            NewProjectDialog dialog = new NewProjectDialog()
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
                return;

            ProjectCreationOptions options = new()
            {
                ParentDirectory = dialog.SelectedLocation,
                ProjectName = dialog.SanitizedProjectName,
                ProjectType = dialog.SelectedProjectType,
                CreateVirtualHost = dialog.CreateVirtualHost,
                VirtualHostDomain = dialog.VirtualHostDomain,
                EnableHttps = dialog.EnableHttps,
                CreateDatabase = dialog.CreateDatabase,
                DatabaseName = dialog.DatabaseName
            };

            projectCreationWindow = new ProjectCreationWindow()
            {
                Owner = this
            };

            projectCreationWindow.Show();

            bool created =
                await projectCreationManager.CreateProject(options);

            if (!created)
            {
                projectCreationWindow?.SetFailed(
                    "Check the log above for the Composer error."
                );

                return;
            }

            projectCreationWindow?.SetComplete();

            projectCreationWindow = null;

            RefreshProjectTree();

            await LoadDockedPreview();
        }

        private async void Window_PreviewKeyDown(
            object sender,
            System.Windows.Input.KeyEventArgs e)
        {
            if (
                e.Key == System.Windows.Input.Key.N &&
                System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control
            )
            {
                await OpenNewProject();

                e.Handled = true;

                return;
            }

            if (
                e.Key == System.Windows.Input.Key.O &&
                System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control
            )
            {
                OpenProjectButton_Click(
                    this,
                    new RoutedEventArgs()
                );

                e.Handled = true;

                return;
            }

            if (e.Key ==
                System.Windows.Input.Key.S &&
                System.Windows.Input.Keyboard.Modifiers ==
                (
                    System.Windows.Input.ModifierKeys.Control |
                    System.Windows.Input.ModifierKeys.Shift
                )
            )
            {
                SaveAsButton_Click(
                    this,
                    new RoutedEventArgs()
                );

                e.Handled = true;

                return;
            }

            if (e.Key ==
                System.Windows.Input.Key.S &&
                System.Windows.Input.Keyboard.Modifiers ==
                System.Windows.Input.ModifierKeys.Control
            )
            {
                bool saved =
                    editorManager.SaveActiveDocument();

                if (saved)
                {
                    if (previewWindow != null)
                    {
                        await previewWindow.RefreshPreview();
                    }

                    await RefreshDockedPreview();
                }

                EditorTabs.SetDocuments(
                    editorManager.OpenDocuments
                );

                e.Handled =
                    true;
            }
        }

        private void Window_Closing(
            object? sender,
            System.ComponentModel.CancelEventArgs e)
        {
            foreach (
                OpenDocument document
                in editorManager.OpenDocuments.ToList())
            {
                if (!CanCloseDocument(document))
                {
                    e.Cancel =
                        true;

                    return;
                }
            }
        }

        private void DeveloperToolsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DeveloperToolsWindow window =
                new DeveloperToolsWindow(
                    developerToolsManager,
                    apacheManager,
                    installationPaths,
                    magentoManager)
                {
                    Owner = this
                };

            window.Show();
        }

        private const string NoProjectPage = @"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                body {
                    background:#1E1E1E;
                    color:#CCCCCC;
                    font-family:'Segoe UI';
                    height:100vh;
                    display:flex;
                    justify-content:center;
                    align-items:center;
                    margin:0;
                }
                .container {
                    text-align:center;
                }
                .title {
                    font-size:22px;
                }
                .text {
                    color:#888888;
                    margin-top:10px;
                }
                </style>
            </head>

            <body>
                <div class='container'>
                    <div class='title'>No project loaded</div>
                    <div class='text'>Open or create a project to start previewing.</div>
                </div>
            </body>
            </html>
        ";

        private async Task ShowNoProjectLoaded()
        {
            await LivePreviewBrowser.EnsureCoreWebView2Async();

            LivePreviewBrowser.NavigationStarting += LivePreviewBrowser_NavigationStarting;

            LivePreviewBrowser.NavigationCompleted += LivePreviewBrowser_NavigationCompleted;

            LivePreviewBrowser.NavigateToString(NoProjectPage);
        }

        private void LivePreviewBrowser_NavigationStarting(
            object? sender,            Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            ClearOutput();

            WriteOutput(
                $"Loading: {e.Uri}"
            );
        }

        private async Task LoadDockedPreview()
        {
            string? projectPath =
                projectManager.CurrentProjectPath;

            if (string.IsNullOrEmpty(projectPath))
            {
                await ShowNoProjectLoaded();
                return;
            }

            ProjectSettings? settings = phpPreviewManager.LoadProjectSettings(
                    projectPath
            );

            string? previewUrl = null;

            if (settings != null &&
                !string.IsNullOrWhiteSpace(settings.Domain))
            {
                string protocol = settings.Ssl? "https" : "http";

                previewUrl = $"{protocol}://{settings.Domain}/";
            }
            else
            {
                string? previewFilePath = phpPreviewManager.SelectPreviewEntryPoint(
                    projectPath
                );

                if (string.IsNullOrEmpty(previewFilePath))
                    return;

                bool started =
                    await phpPreviewManager.StartPhpServer(
                        projectPath
                    );

                if (!started)
                    return;

                previewUrl = phpPreviewManager.GetPreviewUrl(
                    projectPath,
                    previewFilePath
                );
            }

            await LivePreviewBrowser.EnsureCoreWebView2Async();

            if (!webMessageHooked)
            {
                LivePreviewBrowser.CoreWebView2.WebMessageReceived +=
                    LivePreviewBrowser_WebMessageReceived;

                webMessageHooked = true;
            }

            LivePreviewBrowser.Source = new Uri(previewUrl);
        }

        private void LivePreviewBrowser_WebMessageReceived(
            object? sender,            Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            WriteOutput(
                e.TryGetWebMessageAsString()
            );
        }

        private void LivePreviewBrowser_NavigationCompleted(
            object? sender,            Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
                return;
        }

        private async Task RefreshDockedPreview()
        {
            if (LivePreviewBrowser.CoreWebView2 != null)
            {
                ClearOutput();

                await LivePreviewBrowser.CoreWebView2.CallDevToolsProtocolMethodAsync(
                    "Page.reload",
                    "{\"ignoreCache\":true}"
                );
            }
        }

        private void ClearOutput()
        {
            OutputPanel.Document.Blocks.Clear();
        }

        private void WriteOutput(string message)
        {
            Dispatcher.Invoke(() =>
            {
                Brush colour =
                    System.Windows.Media.Brushes.LightGray;

                if (message.StartsWith("[ERROR]"))
                {
                    colour =
                        System.Windows.Media.Brushes.IndianRed;
                }
                else if (message.StartsWith("[WARN]"))
                {
                    colour =
                        System.Windows.Media.Brushes.Gold;
                }
                else if (message.StartsWith("[LOG]"))
                {
                    colour =
                        System.Windows.Media.Brushes.LightGreen;
                }
                else if (message.StartsWith("Loading:"))
                {
                    colour =
                        System.Windows.Media.Brushes.DeepSkyBlue;
                }

                Paragraph paragraph = new();

                paragraph.Inlines.Add(
                    new Run(message)
                    {
                        Foreground = colour
                    }
                );

                OutputPanel.Document.Blocks.Add(
                    paragraph
                );

                OutputPanel.ScrollToEnd();
            });
        }
    }
}