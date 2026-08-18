using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace WAMP_DS.Views
{
    public partial class PreviewWindow : Window
    {
        private readonly string previewUrl;

        public PreviewWindow(string previewUrl)
        {
            InitializeComponent();

            this.previewUrl = previewUrl;

            Loaded += PreviewWindow_Loaded;
        }

        private async void PreviewWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                await Browser.EnsureCoreWebView2Async();


                Browser.NavigationStarting +=
                    Browser_NavigationStarting;


                Browser.NavigationCompleted +=
                    Browser_NavigationCompleted;


                LoadPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to initialise the preview.\n\n{ex.Message}",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void Browser_NavigationStarting(
    object? sender,
    Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            ShowLoading();
        }


        private void Browser_NavigationCompleted(
            object? sender,
            Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            HideLoading();
            
            if (Browser.Source != null)
            {
                UrlTextBox.Text =
                    Browser.Source.ToString();
            }
        }

        private void LoadPage()
        {
            if (string.IsNullOrEmpty(previewUrl))
                return;

            UrlTextBox.Text =
                previewUrl;

            Browser.Source =
                new Uri(previewUrl);
        }

        public async Task RefreshPreview()
        {
            if (Browser.CoreWebView2 != null)
            {
                await Browser.CoreWebView2.CallDevToolsProtocolMethodAsync(
                    "Page.reload",
                    "{\"ignoreCache\":true}"
                );
            }
        }

        private async void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RefreshPreview();
        }
        private void ShowLoading()
        {
            LoadingOverlay.Visibility =
                Visibility.Visible;
        }

        private void HideLoading()
        {
            LoadingOverlay.Visibility =
                Visibility.Collapsed;
        }

        private void UrlTextBox_KeyDown(
    object sender,
    KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;


            string url =
                UrlTextBox.Text.Trim();


            if (string.IsNullOrEmpty(url))
                return;


            if (!url.StartsWith("http://") &&
                !url.StartsWith("https://"))
            {
                url =
                    "http://" + url;
            }


            try
            {
                Browser.Source =
                    new Uri(url);
            }
            catch
            {
                MessageBox.Show(
                    "Invalid URL.",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }
    }

}