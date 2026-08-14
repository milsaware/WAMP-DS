using System.Windows;
using System.Windows.Controls;

namespace WAMP_DS.Views
{
    public partial class NewFileDialog : Window
    {
        public string SelectedExtension { get; private set; } = string.Empty;

        public string SelectedTemplate { get; private set; } = string.Empty;

        public NewFileDialog()
        {
            InitializeComponent();
        }

        private void FileTypeListBox_SelectionChanged(
    object sender,
    SelectionChangedEventArgs e)
        {
            if (FileTypeListBox.SelectedItem is not ListBoxItem item)
            {
                CreateButton.IsEnabled = false;

                return;
            }

            SelectedExtension =
                item.Tag?.ToString() ?? string.Empty;

            SelectedTemplate =
                GetTemplate(
                    SelectedExtension
                );

            CreateButton.IsEnabled = true;
        }

        private void CreateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private static string GetTemplate(
            string extension)
        {
            return extension switch
            {
                ".php" =>
                    "<?php\n",

                ".html" =>
                    "<!DOCTYPE html>\n" +
                    "<html lang=\"en\">\n" +
                    "<head>\n" +
                    "    <meta charset=\"UTF-8\">\n" +
                    "    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n" +
                    "    <title>Document</title>\n" +
                    "</head>\n" +
                    "<body>\n" +
                    "\n" +
                    "</body>\n" +
                    "</html>\n",

                ".css" =>
                    "/* Stylesheet */\n",

                ".js" =>
                    "'use strict';\n",

                ".json" =>
                    "{\n\n}\n",

                ".xml" =>
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n",

                ".sql" =>
                    "-- SQL Script\n",

                ".cs" =>
                    "namespace WAMP_DS\n" +
                    "{\n" +
                    "    public class NewClass\n" +
                    "    {\n" +
                    "    }\n" +
                    "}\n",

                _ => string.Empty
            };
        }

        private void FileTypeListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (FileTypeListBox.SelectedItem is not ListBoxItem)
                return;

            DialogResult = true;
        }
    }
}