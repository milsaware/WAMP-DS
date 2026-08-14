using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WAMP_DS.Managers;
using WAMP_DS.Models;

namespace WAMP_DS.Views
{
    public partial class ApacheSettingsWindow : Window
    {
        private readonly ApacheManager apacheManager;

        private bool loading;

        public ObservableCollection<ApacheModule> Modules { get; set; }


        public ApacheSettingsWindow(
    ApacheManager apacheManager)
        {
            InitializeComponent();

            this.apacheManager = apacheManager;

            Modules = new ObservableCollection<ApacheModule>();

            foreach (ApacheModule module in apacheManager.GetAvailableModules())
            {
                Modules.Add(module);
            }

            DataContext = this;

            LoadSettings();
        }


        private void LoadSettings()
        {
            loading = true;


            ApacheSettings settings =
                apacheManager.GetSettings();


            HttpEnabledCheckBox.IsChecked =
                settings.HttpEnabled;


            HttpsEnabledCheckBox.IsChecked =
                settings.HttpsEnabled;


            Modules.Clear();


            foreach (ApacheModule module in apacheManager.GetAvailableModules())
            {
                Modules.Add(module);
            }


            loading = false;
        }


        protected override async void OnClosed(EventArgs e)
        {
            try
            {
                await apacheManager.RestartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Apache Restart Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                base.OnClosed(e);
            }
        }


        private void SettingChanged(
    object sender,
    RoutedEventArgs e)
        {
            if (loading)
                return;


            ApacheSettings settings =
                new ApacheSettings
                {
                    HttpEnabled =
                        HttpEnabledCheckBox.IsChecked == true,

                    HttpsEnabled =
                        HttpsEnabledCheckBox.IsChecked == true
                };


            try
            {
                apacheManager.ApplySettings(settings);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Apache Settings Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                LoadSettings();
            }
        }


        private void ModuleCheckBox_Changed(
    object sender,
    RoutedEventArgs e)
        {
            if (loading)
                return;


            if (sender is not CheckBox checkBox)
                return;


            if (checkBox.DataContext is not ApacheModule module)
                return;


            try
            {
                apacheManager.SetModuleEnabled(
                    module,
                    module.IsEnabled
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Apache Module Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                LoadSettings();
            }
        }


        private void OpenConfigEditorButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ApacheConfigEditorWindow editor =
                new ApacheConfigEditorWindow(
                    apacheManager,
                    apacheManager.ConfigurationFile,
                    "httpd.conf")
                {
                    Owner = this
                };


            editor.Show();
        }

        private void OpenVirtualHostsEditorButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            ApacheConfigEditorWindow editor =
                new ApacheConfigEditorWindow(
                    apacheManager,
                    apacheManager.VirtualHostsConfigurationFile,
                    "httpd-vhosts.conf")
                {
                    Owner = this
                };

            editor.Show();
        }
    }
}