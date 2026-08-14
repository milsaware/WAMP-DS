using System.Windows;
using WAMP_DS.Models;

namespace WAMP_DS.Views
{
    public partial class WordPressSettingsWindow : Window
    {
        public WordPressInstallSettings? Settings { get; private set; } = new();

        public WordPressSettingsWindow()
        {
            InitializeComponent();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            Settings =
                new WordPressInstallSettings
                {
                    SiteTitle = SiteTitleTextBox.Text,
                    AdminUsername = AdminUsernameTextBox.Text,
                    AdminPassword = AdminPasswordBox.Password,
                    AdminEmail = AdminEmailTextBox.Text,
                    DiscourageSearchEngines =
                        DiscourageSearchEnginesCheckBox.IsChecked == true
                };

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}