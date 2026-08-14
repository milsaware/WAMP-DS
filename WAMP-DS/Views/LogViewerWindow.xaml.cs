using System.IO;
using System.Windows;

namespace WAMP_DS.Views
{
    public partial class LogViewerWindow : Window
    {
        private readonly string logFile;


        public LogViewerWindow(string apachePath)
        {
            InitializeComponent();


            logFile = Path.Combine(
                apachePath,
                "logs",
                "error_log"
            );


            LoadLog();
        }


        private void Refresh_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadLog();
        }


        private void LoadLog()
        {
            if (!File.Exists(logFile))
            {
                LogTextBox.Text =
                    "Apache log file does not exist yet.";

                return;
            }

            try
            {
                using FileStream stream =
                    new FileStream(
                        logFile,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite
                    );

                using StreamReader reader =
                    new StreamReader(stream);

                LogTextBox.Text =
                    reader.ReadToEnd();
            }
            catch (IOException ex)
            {
                LogTextBox.Text =
                    $"Unable to read Apache log:\n\n{ex.Message}";
            }
        }

        private void Clear_Click(
    object sender,
    RoutedEventArgs e)
        {
            if (!File.Exists(logFile))
                return;


            MessageBoxResult result =
                MessageBox.Show(
                    "Are you sure you want to clear the Apache error log?\n\nThis cannot be undone.",
                    "Clear Apache Log",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );


            if (result != MessageBoxResult.Yes)
            {
                return;
            }


            try
            {
                using FileStream stream =
                    new FileStream(
                        logFile,
                        FileMode.Open,
                        FileAccess.Write,
                        FileShare.ReadWrite
                    );

                stream.SetLength(0);
                stream.Flush();

                LoadLog();
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    $"Unable to clear Apache log:\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }
    }
}