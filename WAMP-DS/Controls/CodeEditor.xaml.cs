using System;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace WAMP_DS.Controls
{
    public partial class CodeEditor : UserControl
    {
        public event EventHandler? TextChanged;


        public CodeEditor()
        {
            InitializeComponent();

            EditorTextBox.Options.EnableHyperlinks = false;
            EditorTextBox.Options.EnableEmailHyperlinks = false;

            EditorTextBox.TextChanged += EditorTextBox_TextChanged;
        }


        public string Text
        {
            get => EditorTextBox.Text;
            set => EditorTextBox.Text = value;
        }


        public void SetLanguage(
            string extension)
        {
            IHighlightingDefinition? highlighting =
                HighlightingManager.Instance
                    .GetDefinitionByExtension(extension);

            EditorTextBox.SyntaxHighlighting =
                highlighting;
        }


        private void EditorTextBox_TextChanged(
            object? sender,
            EventArgs e)
        {
            TextChanged?.Invoke(
                this,
                EventArgs.Empty
            );
        }
    }
}