using System;
using System.Windows;

namespace WAMP_DS.Views
{
    public partial class PhpMyAdminWindow : Window
    {
        private const string PhpMyAdminUrl =
            "http://localhost/phpmyadmin/";

        public PhpMyAdminWindow()
        {
            InitializeComponent();

            Loaded += PhpMyAdminWindow_Loaded;
        }

        private async void PhpMyAdminWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                await Browser.EnsureCoreWebView2Async();

                LoadPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to initialise phpMyAdmin.\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void LoadPage()
        {
            Browser.Source =
                new Uri(PhpMyAdminUrl);
        }

        public void RefreshPhpMyAdmin()
        {
            if (Browser.CoreWebView2 != null)
            {
                Browser.Reload();
            }
        }

        private void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            RefreshPhpMyAdmin();
        }
    }
}