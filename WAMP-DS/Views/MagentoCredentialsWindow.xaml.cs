using System.Windows;
using WAMP_DS.Core;
using WAMP_DS.Models;

namespace WAMP_DS.Views
{
    public partial class MagentoCredentialsWindow : Window
    {
        private readonly MagentoCredentialManager credentialManager;

        public MagentoCredentialsWindow()
        {
            InitializeComponent();

            credentialManager = new MagentoCredentialManager();

            LoadCredentials();
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void LoadCredentials()
        {
            MagentoSettings settings = credentialManager.Load();

            PublicKeyTextBox.Text = settings.PublicKey;

            PrivateKeyTextBox.Password = settings.PrivateKey;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MagentoSettings settings = new()
            {
                PublicKey = PublicKeyTextBox.Text.Trim(),

                PrivateKey = PrivateKeyTextBox.Password.Trim()
            };

            if (string.IsNullOrWhiteSpace(settings.PublicKey) ||
                string.IsNullOrWhiteSpace(settings.PrivateKey))
            {
                MessageBox.Show(
                    "Please enter both Magento keys.",
                    "Magento Credentials",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            credentialManager.Save(
                settings
            );

            DialogResult = true;

            Close();
        }
    }
}