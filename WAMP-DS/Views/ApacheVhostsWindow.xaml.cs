using System.Windows;
using System.Collections.Generic;
using WAMP_DS.Managers;
using WAMP_DS.Models;
using System.Linq;

namespace WAMP_DS.Views
{
    public partial class ApacheVhostsWindow : Window
    {
        private readonly ApacheManager apacheManager;

        private ApacheVirtualHost? selectedHost;


        public ApacheVhostsWindow(
            ApacheManager apacheManager)
        {
            InitializeComponent();

            this.apacheManager =
                apacheManager;

            LoadVirtualHosts();
        }



        private void LoadVirtualHosts()
        {
            List<ApacheVirtualHost> hosts =
                apacheManager.ReadVirtualHosts();


            VirtualHostsList.Items.Clear();


            foreach (ApacheVirtualHost host in hosts)
            {
                VirtualHostsList.Items.Add(host);
            }
        }



        private void AddHostButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ApacheVirtualHost host =
                new ApacheVirtualHost
                {
                    ServerName = "new-site.local",
                    DocumentRoot = "",
                    Directory = "",

                    HttpsEnabled = false,

                    AllowOverride = "All",
                    RequireValue = "all granted",

                    OptionsIndexes = false,
                    OptionsFollowSymLinks = true,
                    OptionsExecCGI = false,
                    OptionsIncludes = false,
                    OptionsMultiViews = false,

                    RewriteEngine = false,

                    ErrorLog = "logs/new-site-error.log",
                    CustomLog = "logs/new-site-access.log"
                };


            VirtualHostsList.Items.Add(
                host
            );


            VirtualHostsList.SelectedItem =
                host;
        }



        private void RemoveHostButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (VirtualHostsList.SelectedItem == null)
                return;


            VirtualHostsList.Items.Remove(
                VirtualHostsList.SelectedItem
            );


            ClearFields();
        }



        private void VirtualHostsList_SelectionChanged(
            object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedHost =
                VirtualHostsList.SelectedItem
                as ApacheVirtualHost;


            if (selectedHost == null)
                return;


            LoadHost();
        }



        private void LoadHost()
        {
            ServerNameText.Text =
                selectedHost!.ServerName;


            DocumentRootText.Text =
                selectedHost.DocumentRoot;


            DirectoryText.Text =
                selectedHost.Directory;


            HttpsEnabledCheckBox.IsChecked =
                selectedHost.HttpsEnabled;


            AllowOverrideComboBox.SelectedValue =
                selectedHost.AllowOverride;


            RequireComboBox.SelectedValue =
                selectedHost.RequireValue;


            OptionsIndexesCheckBox.IsChecked =
                selectedHost.OptionsIndexes;


            OptionsFollowSymLinksCheckBox.IsChecked =
                selectedHost.OptionsFollowSymLinks;


            OptionsExecCGICheckBox.IsChecked =
                selectedHost.OptionsExecCGI;


            OptionsIncludesCheckBox.IsChecked =
                selectedHost.OptionsIncludes;


            OptionsMultiViewsCheckBox.IsChecked =
                selectedHost.OptionsMultiViews;


            RewriteEngineCheckBox.IsChecked =
                selectedHost.RewriteEngine;


            ErrorLogText.Text =
                selectedHost.ErrorLog;


            CustomLogText.Text =
                selectedHost.CustomLog;
        }



        private async void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (selectedHost == null)
                return;


            selectedHost.ServerName =
                ServerNameText.Text;


            selectedHost.DocumentRoot =
                DocumentRootText.Text;


            selectedHost.Directory =
                DirectoryText.Text;


            selectedHost.HttpsEnabled =
                HttpsEnabledCheckBox.IsChecked == true;


            selectedHost.AllowOverride =
                AllowOverrideComboBox.SelectedValue?.ToString()
                ?? "All";


            selectedHost.RequireValue =
                RequireComboBox.SelectedValue?.ToString()
                ?? "None";


            selectedHost.OptionsIndexes =
                OptionsIndexesCheckBox.IsChecked == true;


            selectedHost.OptionsFollowSymLinks =
                OptionsFollowSymLinksCheckBox.IsChecked == true;


            selectedHost.OptionsExecCGI =
                OptionsExecCGICheckBox.IsChecked == true;


            selectedHost.OptionsIncludes =
                OptionsIncludesCheckBox.IsChecked == true;


            selectedHost.OptionsMultiViews =
                OptionsMultiViewsCheckBox.IsChecked == true;


            selectedHost.RewriteEngine =
                RewriteEngineCheckBox.IsChecked == true;


            selectedHost.ErrorLog =
                ErrorLogText.Text;


            selectedHost.CustomLog =
                CustomLogText.Text;


            VirtualHostsList.Items.Refresh();


            List<ApacheVirtualHost> hosts =
                VirtualHostsList.Items
                .Cast<ApacheVirtualHost>()
                .ToList();


            await apacheManager.SaveVirtualHosts(hosts);


            MessageBox.Show(
                "Virtual host updated.",
                "WAMP-DS",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }



        private void ClearFields()
        {
            ServerNameText.Text = "";
            DocumentRootText.Text = "";
            DirectoryText.Text = "";

            HttpsEnabledCheckBox.IsChecked = false;

            AllowOverrideComboBox.SelectedIndex = 0;

            RequireComboBox.SelectedIndex = 0;

            OptionsIndexesCheckBox.IsChecked = false;
            OptionsFollowSymLinksCheckBox.IsChecked = false;
            OptionsExecCGICheckBox.IsChecked = false;
            OptionsIncludesCheckBox.IsChecked = false;
            OptionsMultiViewsCheckBox.IsChecked = false;

            RewriteEngineCheckBox.IsChecked = false;

            ErrorLogText.Text = "";
            CustomLogText.Text = "";
        }

    }
}