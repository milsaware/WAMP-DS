using System.IO;
using System.Windows;
using WAMP_DS.Managers;

namespace WAMP_DS.Views
{
    public partial class ApacheConfigEditorWindow : Window
    {
        private readonly ApacheManager apacheManager;
        private readonly string configurationFile;
        private readonly string configurationName;


        public ApacheConfigEditorWindow(
            ApacheManager apacheManager,
            string configurationFile,
            string configurationName)
        {
            InitializeComponent();

            this.apacheManager = apacheManager;
            this.configurationFile = configurationFile;
            this.configurationName = configurationName;

            Title = $"{configurationName} Editor";

            LoadConfiguration();
        }


        private void LoadConfiguration()
        {
            if (!File.Exists(configurationFile))
            {
                ConfigEditor.Text =
                    $"Configuration file not found:\n\n{configurationFile}";

                return;
            }


            ConfigEditor.Text =
                File.ReadAllText(
                    configurationFile
                );
        }


        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText(
                    configurationFile,
                    ConfigEditor.Text
                );


                MessageBox.Show(
                    $"{configurationName} saved.",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        private void ValidateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                apacheManager.ValidateConfiguration();


                MessageBox.Show(
                    "Apache configuration is valid.",
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "WAMP-DS",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}