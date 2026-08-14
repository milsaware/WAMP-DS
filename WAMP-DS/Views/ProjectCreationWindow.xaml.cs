using System.Text;
using System.Windows;

namespace WAMP_DS.Views
{
    public partial class ProjectCreationWindow : Window
    {
        private bool detailsVisible;

        private readonly StringBuilder log =
            new();

        private DateTime lastLogUpdate = DateTime.MinValue;

        public ProjectCreationWindow()
        {
            InitializeComponent();
        }

        private void UpdateLogDisplay()
        {
            DetailsTextBox.Text =
                log.ToString();

            DetailsTextBox.ScrollToEnd();

            lastLogUpdate = DateTime.Now;
        }

        public void UpdateStatus(string message)
        {
            log.AppendLine(message);

            Dispatcher.BeginInvoke(() =>
            {
                StatusText.Text = message;

                DetailsTextBox.Text =
                    log.ToString();

                DetailsTextBox.ScrollToEnd();
            });
        }

        public void AddLog(string message)
        {
            log.AppendLine(message);

            Dispatcher.BeginInvoke(() =>
            {
                DetailsTextBox.Text =
                    log.ToString();

                DetailsTextBox.ScrollToEnd();
            });
        }

        private void DetailsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            detailsVisible =
                !detailsVisible;

            if (detailsVisible)
            {
                DetailsTextBox.Visibility =
                    Visibility.Visible;


                DetailsButton.Content =
                    "Hide Details";

                Height =
                    500;
            }
            else
            {
                DetailsTextBox.Visibility =
                    Visibility.Collapsed;


                DetailsButton.Content =
                    "Show Details";


                Height =
                    200;
            }
        }
        public void KeepOpen()
        {
            Dispatcher.BeginInvoke(() =>
            {
                ProgressBar.Visibility =
                    Visibility.Collapsed;

                CloseButton.Visibility =
                    Visibility.Visible;

                StatusText.Text =
                    "Project creation failed. See details below.";
            });
        }
        public void SetComplete()
        {
            Dispatcher.BeginInvoke(() =>
            {
                StatusText.Text =
                    "Project created successfully.";


                ProgressBar.Visibility =
                    Visibility.Collapsed;


                CloseButton.Visibility =
                    Visibility.Visible;
            });
        }

        public void SetFailed(string message)
        {
            Dispatcher.BeginInvoke(() =>
            {
                StatusText.Text =
                    "Project creation failed.";


                ProgressBar.Visibility =
                    Visibility.Collapsed;


                AddLog(
                    message
                );


                CloseButton.Visibility =
                    Visibility.Visible;
            });
        }

        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}