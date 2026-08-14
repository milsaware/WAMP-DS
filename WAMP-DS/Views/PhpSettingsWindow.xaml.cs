using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WAMP_DS.Managers;
using System.Linq;
using WAMP_DS.Models;

namespace WAMP_DS.Views
{
    public partial class PhpSettingsWindow : Window
    {
        private readonly PhpSettingsManager phpSettingsManager;

        private readonly ApacheManager apacheManager;

        private readonly MySQLManager mySQLManager;

        private readonly string phpDirectory;

        private bool isResettingServices;

        public PhpSettingsWindow(
            ApacheManager apacheManager,
            MySQLManager mySQLManager,
            string phpDirectory)
        {
            InitializeComponent();

            this.apacheManager =
                apacheManager;

            this.mySQLManager =
                mySQLManager;

            this.phpDirectory =
                phpDirectory;

            phpSettingsManager =
                new PhpSettingsManager(
                    phpDirectory
                );

            Loaded +=
                PhpSettingsWindow_Loaded;

            Closing +=
                PhpSettingsWindow_Closing;
        }


        // ============================================================
        // WINDOW LOADED
        // ============================================================

        private void PhpSettingsWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            PhpIniPathText.Text =
                phpSettingsManager.PhpIniPath;

            LoadSections();

            LoadPhpSettings();
        }


        // ============================================================
        // WINDOW CLOSING
        // ============================================================

        private async void PhpSettingsWindow_Closing(
            object? sender,
            System.ComponentModel.CancelEventArgs e)
        {
            if (isResettingServices)
            {
                return;
            }


            isResettingServices =
                true;


            try
            {
                // ----------------------------------------------------
                // STOP APACHE
                // ----------------------------------------------------

                if (apacheManager.IsRunning)
                {
                    apacheManager.Stop();
                    await apacheManager.StartAsync();
                }


                // ----------------------------------------------------
                // STOP MYSQL
                // ----------------------------------------------------

                if (mySQLManager.IsRunning)
                {
                    mySQLManager.Stop();
                }


                // ----------------------------------------------------
                // START MYSQL
                // ----------------------------------------------------

                await mySQLManager.StartAsync();


                // ----------------------------------------------------
                // START APACHE
                // ----------------------------------------------------

                await apacheManager.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"PHP settings were saved, but WAMP-DS was unable to restart the server services.\n\n" +
                    $"{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }


        // ============================================================
        // LOAD SECTIONS
        // ============================================================

        private void LoadSections()
        {
            try
            {
                SectionListBox.Items.Clear();


                var sections =
                    phpSettingsManager
                        .GetSettings()
                        .GroupBy(
                            x => x.Section,
                            StringComparer.OrdinalIgnoreCase
                        )
                        .Select(
                            x => new PhpSection
                            {
                                Name =
                                    x.Key,

                                SettingCount =
                                    x.Count()
                            }
                        )
                        .ToList();


                foreach (PhpSection section in sections)
                {
                    var panel =
                        new StackPanel();


                    var nameText =
                        new TextBlock
                        {
                            Text =
                                section.Name,

                            Foreground =
                                Brushes.LightGray,

                            FontSize =
                                13
                        };


                    var countText =
                        new TextBlock
                        {
                            Text =
                                $"{section.SettingCount} settings",

                            Foreground =
                                Brushes.Gray,

                            FontSize =
                                11,

                            Margin =
                                new Thickness(
                                    0,
                                    3,
                                    0,
                                    0
                                )
                        };


                    panel.Children.Add(
                        nameText
                    );


                    panel.Children.Add(
                        countText
                    );


                    var item =
                        new ListBoxItem
                        {
                            Content =
                                panel,

                            Tag =
                                section.Name,

                            Padding =
                                new Thickness(
                                    10
                                )
                        };


                    SectionListBox.Items.Add(
                        item
                    );
                }


                if (SectionListBox.Items.Count > 0)
                {
                    SectionListBox.SelectedIndex =
                        0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load PHP configuration sections.\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        // ============================================================
        // LOAD PHP SETTINGS
        // ============================================================

        private void LoadPhpSettings()
        {
            try
            {
                if (!File.Exists(
                    phpSettingsManager.PhpIniPath))
                {
                    MessageBox.Show(
                        $"The PHP configuration file could not be found.\n\n" +
                        $"{phpSettingsManager.PhpIniPath}",
                        "WAMP-DS",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    return;
                }


                SettingsPanel.Children.Clear();


                string searchText =
                    SearchTextBox.Text?
                        .Trim() ??
                    string.Empty;


                string selectedSection =
                    GetSelectedSection();


                UpdateSectionHeader(
                    selectedSection
                );


                var settings =
                    phpSettingsManager.GetSettings();


                foreach (var setting in settings)
                {
                    if (!MatchesSearch(
                        setting,
                        searchText))
                    {
                        continue;
                    }


                    if (!MatchesSection(
                        setting,
                        selectedSection))
                    {
                        continue;
                    }


                    SettingsPanel.Children.Add(
                        CreateSettingCard(
                            setting
                        )
                    );
                }


                NoSettingsText.Visibility =
                    SettingsPanel.Children.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load PHP configuration.\n\n" +
                    $"{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        // ============================================================
        // UPDATE SECTION HEADER
        // ============================================================

        private void UpdateSectionHeader(
            string section)
        {
            SectionTitleText.Text =
                section;

            SectionDescriptionText.Text =
                GetSectionDescription(
                    section
                );
        }


        // ============================================================
        // SECTION DESCRIPTION
        // ============================================================

        private string GetSectionDescription(
            string section)
        {
            return section switch
            {
                "Global" =>
                    "Global PHP runtime configuration.",

                "CLI Server" =>
                    "Configuration for the PHP built-in development server.",

                "Date" =>
                    "Date, time and timezone configuration.",

                "filter" =>
                    "Input filtering and validation configuration.",

                "iconv" =>
                    "Character encoding conversion configuration.",

                "intl" =>
                    "Internationalisation and locale configuration.",

                "sqlite3" =>
                    "SQLite3 database configuration.",

                "Pcre" =>
                    "Perl-compatible regular expression configuration.",

                "Pdo" =>
                    "PHP Data Objects configuration.",

                "Pdo_mysql" =>
                    "MySQL PDO driver configuration.",

                "Phar" =>
                    "PHP Archive configuration.",

                "mail function" =>
                    "PHP mail and SMTP configuration.",

                "ODBC" =>
                    "ODBC database connection configuration.",

                "MySQLi" =>
                    "MySQL Improved extension configuration.",

                "mysqlnd" =>
                    "MySQL Native Driver configuration.",

                "PostgreSQL" =>
                    "PostgreSQL database connection configuration.",

                "bcmath" =>
                    "Arbitrary precision mathematics configuration.",

                "browscap" =>
                    "Browser capabilities configuration.",

                "Session" =>
                    "PHP session and session cookie configuration.",

                "Assertion" =>
                    "PHP assertion and debugging configuration.",

                "COM" =>
                    "Windows COM and .NET interoperability configuration.",

                "mbstring" =>
                    "Multibyte string and character encoding configuration.",

                "gd" =>
                    "GD image processing configuration.",

                "exif" =>
                    "EXIF metadata and image information configuration.",

                "Tidy" =>
                    "HTML cleanup and formatting configuration.",

                "soap" =>
                    "SOAP and WSDL configuration.",

                "sysvshm" =>
                    "System V shared memory configuration.",

                "ldap" =>
                    "LDAP connection configuration.",

                "dba" =>
                    "Database abstraction layer configuration.",

                "opcache" =>
                    "PHP opcode caching and performance configuration.",

                "curl" =>
                    "cURL SSL certificate configuration.",

                "openssl" =>
                    "OpenSSL and certificate authority configuration.",

                "ffi" =>
                    "Foreign Function Interface configuration.",

                _ =>
                    "PHP runtime configuration."
            };
        }


        // ============================================================
        // CREATE SETTING CARD
        // ============================================================

        private Border CreateSettingCard(PhpSetting setting)
        {
            var border =
                new Border
                {
                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                30,
                                30,
                                30
                            )
                        ),

                    BorderBrush =
                        new SolidColorBrush(
                            Color.FromRgb(
                                63,
                                63,
                                70
                            )
                        ),

                    BorderThickness =
                        new Thickness(1),

                    Padding =
                        new Thickness(12),

                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            8
                        ),

                    Tag =
                        setting
                };


            var details =
                new StackPanel();


            var keyText =
                new TextBlock
                {
                    Text =
                        setting.Name,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                204,
                                204,
                                204
                            )
                        ),

                    FontSize =
                        13
                };


            details.Children.Add(
                keyText
            );


            FrameworkElement control =
                CreateSettingControl(
                    setting
                );


            control.Margin =
                new Thickness(
                    0,
                    8,
                    0,
                    0
                );


            details.Children.Add(
                control
            );


            border.Child =
                details;


            border.MouseEnter +=
                SettingCard_MouseEnter;


            border.MouseLeave +=
                SettingCard_MouseLeave;


            return border;
        }

        private FrameworkElement CreateSettingControl(
    PhpSetting setting)
        {
            switch (setting.Type)
            {
                case PhpSettingType.Boolean:
                case PhpSettingType.Extension:

                    var toggle =
                        new CheckBox
                        {
                            Content =
                                setting.IsEnabled
                                    ? "Enabled"
                                    : "Disabled",

                            IsChecked =
                                setting.IsEnabled,

                            Foreground =
                                Brushes.LightGreen
                        };


                    toggle.Checked += (s, e) =>
                    {
                        phpSettingsManager.SetEnabled(
                            setting.Section,
                            setting.Name,
                            true
                        );

                        phpSettingsManager.Save();

                        toggle.Content =
                            "Enabled";
                    };


                    toggle.Unchecked += (s, e) =>
                    {
                        phpSettingsManager.SetEnabled(
                            setting.Section,
                            setting.Name,
                            false
                        );

                        phpSettingsManager.Save();

                        toggle.Content =
                            "Disabled";
                    };


                    return toggle;

                case PhpSettingType.Timezone:

                    var timezoneBox =
                        new ComboBox
                        {
                            Width = 250,

                            Style =
                                (Style)FindResource(
                                    "DarkComboBox"
                                ),

                            ItemsSource =
                                PhpTimezones.All,

                            SelectedItem =
                                setting.Value
                        };


                    timezoneBox.SelectionChanged += (s, e) =>
                    {
                        if (timezoneBox.SelectedItem == null)
                        {
                            return;
                        }

                        phpSettingsManager.SetValue(
                            setting.Section,
                            setting.Name,
                            timezoneBox.SelectedItem.ToString() ?? string.Empty
                        );

                        phpSettingsManager.Save();
                    };


                    return timezoneBox;

                case PhpSettingType.String:

                    if (setting.Name.Equals(
                            "date.timezone",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return CreateTimezoneSelector(setting);
                    }

                    goto case PhpSettingType.Path;

                case PhpSettingType.Integer:
                case PhpSettingType.Path:

                    var textBox =
                        new TextBox
                        {
                            Text =
                                setting.Value,

                            Background =
                                new SolidColorBrush(
                                    Color.FromRgb(
                                        45,
                                        45,
                                        48
                                    )
                                ),

                            Foreground =
                                Brushes.White,

                            BorderBrush =
                                new SolidColorBrush(
                                    Color.FromRgb(
                                        80,
                                        80,
                                        80
                                    )
                                ),

                            Padding =
                                new Thickness(
                                    6
                                )
                        };


                    textBox.LostFocus += (s, e) =>
                    {
                        phpSettingsManager.SetValue(
                            setting.Section,
                            setting.Name,
                            textBox.Text
                        );

                        phpSettingsManager.Save();
                    };


                    return textBox;


                default:

                    return new TextBlock
                    {
                        Text =
                            setting.Value,

                        Foreground =
                            Brushes.Gray
                    };
            }
        }

        private FrameworkElement CreateTimezoneSelector(
    PhpSetting setting)
        {
            var combo =
                new ComboBox
                {
                    Width = 250,

                    Style =
                        (Style)FindResource(
                            "DarkComboBox"
                        )
                };


            string[] timezones =
            {
        "UTC",
        "Europe/London",
        "Europe/Paris",
        "Europe/Berlin",
        "America/New_York",
        "America/Los_Angeles",
        "Asia/Tokyo",
        "Australia/Sydney"
    };


            foreach (string timezone in timezones)
            {
                combo.Items.Add(
                    timezone
                );
            }


            combo.SelectedItem =
                string.IsNullOrWhiteSpace(setting.Value)
                    ? "UTC"
                    : setting.Value;


            combo.SelectionChanged += (s, e) =>
            {
                if (combo.SelectedItem == null)
                {
                    return;
                }


                phpSettingsManager.SetValue(
                    setting.Section,
                    setting.Name,
                    combo.SelectedItem.ToString()!
                );


                phpSettingsManager.Save();
            };


            return combo;
        }


        // ============================================================
        // SETTING CARD CLICK
        // ============================================================

        private void SettingCard_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (sender is not Border border)
            {
                return;
            }


            if (border.Tag is not PhpSetting setting)
            {
                return;
            }


            try
            {
                bool newState =
                    !setting.IsEnabled;


                phpSettingsManager.SetEnabled(
                    setting.Section,
                    setting.Name,
                    newState
                );

                phpSettingsManager.Save();


                LoadPhpSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to change PHP setting.\n\n" +
                    $"{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        // ============================================================
        // HOVER EFFECT
        // ============================================================

        private void SettingCard_MouseEnter(
            object sender,
            MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            45,
                            45,
                            48
                        )
                    );
            }
        }


        private void SettingCard_MouseLeave(
            object sender,
            MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            30,
                            30,
                            30
                        )
                    );
            }
        }


        // ============================================================
        // SEARCH
        // ============================================================

        private bool MatchesSearch(
            PhpSetting setting,
            string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return true;
            }


            return
                setting.Name.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                setting.Value.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase
                );
        }


        // ============================================================
        // SECTION FILTER
        // ============================================================

        private string GetSelectedSection()
        {
            if (SectionListBox.SelectedItem
                is ListBoxItem item)
            {
                return item.Tag?
                    .ToString() ??
                    "Global";
            }


            return "Global";
        }


        private bool MatchesSection(
            PhpSetting setting,
            string section)
        {
            return string.Equals(
                setting.Section,
                section,
                StringComparison.OrdinalIgnoreCase
            );
        }


        // ============================================================
        // SECTION SELECTION
        // ============================================================

        private void SectionListBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (SettingsPanel == null)
            {
                return;
            }


            LoadPhpSettings();
        }


        // ============================================================
        // SEARCH TEXT CHANGED
        // ============================================================

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (SettingsPanel == null)
            {
                return;
            }


            LoadPhpSettings();
        }


        // ============================================================
        // SAVE
        // ============================================================

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}