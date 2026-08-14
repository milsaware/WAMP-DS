using System.Windows;

namespace WAMP_DS.Views
{
    public partial class NewFolderDialog : Window
    {
        public string FolderName { get; private set; } = string.Empty;


        public NewFolderDialog()
        {
            InitializeComponent();
        }


        private void CreateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            FolderName =
                FolderNameTextBox.Text.Trim();


            if (string.IsNullOrWhiteSpace(FolderName))
                return;

            DialogResult = true;
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}