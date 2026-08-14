using System.Windows;
using WAMP_DS.Managers;
using WAMP_DS.Models;

namespace WAMP_DS.Views
{
    public partial class MagentoSettingsWindow : Window
    {
        public MagentoInstallSettings? Settings { get; private set; }
        private readonly MagentoLanguageManager languageManager;
        private readonly MagentoTimezoneManager timezoneManager;
        private readonly MagentoCurrencyManager currencyManager;

        public MagentoSettingsWindow(string defaultUrl)
        {
            InitializeComponent();

            languageManager = new MagentoLanguageManager();
            timezoneManager = new MagentoTimezoneManager();

            LanguageComboBox.ItemsSource = languageManager.GetLanguages();

            LanguageComboBox.DisplayMemberPath = "Name";
            LanguageComboBox.SelectedValuePath = "Code";
            LanguageComboBox.SelectedValue = "en_GB";

            TimeZoneComboBox.ItemsSource = timezoneManager.GetTimezones();

            TimeZoneComboBox.DisplayMemberPath = "Name";
            TimeZoneComboBox.SelectedValuePath = "Code";
            TimeZoneComboBox.SelectedValue = "Europe/London";

            currencyManager = new MagentoCurrencyManager();

            CurrencyComboBox.ItemsSource = currencyManager.GetCurrencies();

            CurrencyComboBox.DisplayMemberPath = "Name";
            CurrencyComboBox.SelectedValuePath = "Code";
            CurrencyComboBox.SelectedValue = "GBP";

            AdminUrlTextBox.Text = defaultUrl;
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            Settings = new MagentoInstallSettings
            {
                AdminUsername = AdminUsernameTextBox.Text,
                AdminPassword = AdminPasswordBox.Password,
                AdminEmail = AdminEmailTextBox.Text,
                AdminFirstName = AdminFirstNameTextBox.Text,
                AdminLastName = AdminLastNameTextBox.Text,
                AdminUrl = AdminUrlTextBox.Text,
                Language = LanguageComboBox.SelectedValue?.ToString() ?? string.Empty,
                Timezone = TimeZoneComboBox.SelectedValue?.ToString() ?? string.Empty,
                Currency = CurrencyComboBox.SelectedValue?.ToString() ?? string.Empty
            };

            DialogResult = true;
            Close();
        }
    }
}