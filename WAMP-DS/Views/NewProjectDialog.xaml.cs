using System.IO;
using System.Windows;
using Microsoft.Win32;
using WAMP_DS.Models;

namespace WAMP_DS.Views
{
    public partial class NewProjectDialog : Window
    {
        public string ProjectName =>
            ProjectNameTextBox.Text.Trim();

        public string SelectedLocation =>
            LocationTextBox.Text.Trim();

        public ProjectType SelectedProjectType
        {
            get
            {
                if (MagentoProjectRadioButton.IsChecked == true)
                    return ProjectType.Magento;

                if (WordPressProjectRadioButton.IsChecked == true)
                    return ProjectType.WordPress;

                if (LaravelProjectRadioButton.IsChecked == true)
                    return ProjectType.Laravel;

                if (CodeIgniterProjectRadioButton.IsChecked == true)
                    return ProjectType.CodeIgniter;

                if (PhpProjectRadioButton.IsChecked == true)
                    return ProjectType.Php;

                return ProjectType.Html;
            }
        }

        public bool CreateVirtualHost =>
            CreateVirtualHostCheckBox.IsChecked == true;

        public bool EnableHttps =>
            EnableHttpsCheckBox.IsChecked == true;

        public bool CreateDatabase =>
            CreateDatabaseCheckBox.IsChecked == true;

        public string VirtualHostDomain =>
            VirtualHostDomainTextBox.Text.Trim();

        public string DatabaseName =>
            DatabaseNameTextBox.Text.Trim();

        public NewProjectDialog()
        {
            InitializeComponent();
            UpdateDatabaseVisibility();
        }

        private void BrowseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new()
            {
                Title = "Select project location"
            };

            if (dialog.ShowDialog() != true)
                return;

            LocationTextBox.Text =
                dialog.FolderName;
        }

        private void ProjectTypeChanged(
            object sender,
            RoutedEventArgs e)
        {
            UpdateDatabaseVisibility();
        }

        private void UpdateDatabaseVisibility()
        {
            if (CreateDatabaseCheckBox == null)
                return;

            bool supportsDatabase =
                PhpProjectRadioButton.IsChecked == true ||
                CodeIgniterProjectRadioButton.IsChecked == true ||
                LaravelProjectRadioButton.IsChecked == true ||
                WordPressProjectRadioButton.IsChecked == true ||
                MagentoProjectRadioButton.IsChecked == true;


            CreateDatabaseCheckBox.Visibility =
                supportsDatabase
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CreateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            DialogResult = true;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                MessageBox.Show(
                    "Please enter a project name.",
                    "New Project",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                ProjectNameTextBox.Focus();

                return false;
            }

            if (string.IsNullOrWhiteSpace(SelectedLocation))
            {
                MessageBox.Show(
                    "Please select a project location.",
                    "New Project",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            if (!Directory.Exists(SelectedLocation))
            {
                MessageBox.Show(
                    "The selected project location does not exist.",
                    "New Project",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return false;
            }

            return true;
        }

        public string SanitizedProjectName =>
            ProjectName
                .ToLower()
                .Replace(" ", "-");
    }
}