using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;

namespace WAMP_DS.Controls
{
    public partial class CodeEditor : UserControl
    {
        public event EventHandler? TextChanged;

        private static readonly SolidColorBrush SearchBackground =
            new SolidColorBrush(Color.FromRgb(37, 37, 38));

        private static readonly SolidColorBrush SearchForeground =
            new SolidColorBrush(Color.FromRgb(204, 204, 204));

        public CodeEditor()
        {
            InitializeComponent();

            EditorTextBox.Options.EnableHyperlinks = false;
            EditorTextBox.Options.EnableEmailHyperlinks = false;

            EditorTextBox.TextChanged += EditorTextBox_TextChanged;

            PreviewKeyDown += CodeEditor_PreviewKeyDown;
        }

        public string Text
        {
            get => EditorTextBox.Text;
            set => EditorTextBox.Text = value;
        }

        public void SetLanguage(string extension)
        {
            IHighlightingDefinition? highlighting =
                HighlightingManager.Instance
                    .GetDefinitionByExtension(
                        extension
                    );

            EditorTextBox.SyntaxHighlighting = highlighting;
        }

        // ============================================================
        // KEYBOARD SHORTCUTS
        // ============================================================

        private void CodeEditor_PreviewKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (
                e.Key == Key.D &&
                Keyboard.Modifiers == ModifierKeys.Control
            )
            {
                DuplicateCurrentLine();

                e.Handled = true;

                return;
            }

            if (
                e.Key == Key.F &&
                Keyboard.Modifiers == ModifierKeys.Control
            )
            {
                ShowFindPanel();

                e.Handled = true;

                return;
            }

            if (
                e.Key == Key.H &&
                Keyboard.Modifiers == ModifierKeys.Control
            )
            {
                ShowReplacePanel();

                e.Handled = true;

                return;
            }

            if (
                e.Key == Key.Escape &&
                SearchPanel.Visibility ==
                Visibility.Visible
            )
            {
                CloseSearchPanel();

                e.Handled = true;
            }
        }

        // ============================================================
        // CONTEXT MENU
        // ============================================================

        private void ContextFind_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowFindPanel();
        }

        private void ContextReplace_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowReplacePanel();
        }

        private void ContextDuplicate_Click(
            object sender,
            RoutedEventArgs e)
        {
            DuplicateCurrentLine();
        }

        // ============================================================
        // DUPLICATE LINE / SELECTION
        // ============================================================

        private void DuplicateCurrentLine()
        {
            // --------------------------------------------------------
            // If text is selected, duplicate only the selection
            // --------------------------------------------------------

            if (EditorTextBox.SelectionLength > 0)
            {
                int selectionStart = EditorTextBox.SelectionStart;

                int selectionLength = EditorTextBox.SelectionLength;

                string selectedText = EditorTextBox.SelectedText;

                if (string.IsNullOrEmpty(selectedText))
                    return;

                EditorTextBox.Document.Insert(
                    selectionStart + selectionLength,
                    selectedText
                );

                EditorTextBox.Select(
                    selectionStart + selectionLength,
                    selectionLength
                );

                return;
            }

            // --------------------------------------------------------
            // Otherwise duplicate the current line
            // --------------------------------------------------------

            int offset = EditorTextBox.CaretOffset;

            var document = EditorTextBox.Document;

            var line = document.GetLineByOffset(offset);

            string lineText = document.GetText(
                line.Offset,
                line.Length
            );

            int insertOffset = line.EndOffset;

            // Determine the document's newline style

            string newline =
                Environment.NewLine;

            if (document.Text.Contains("\r\n"))
            {
                newline = "\r\n";
            }
            else if (document.Text.Contains("\n"))
            {
                newline = "\n";
            }

            // --------------------------------------------------------
            // Insert the duplicated line
            // --------------------------------------------------------

            document.Insert(
                insertOffset,
                newline + lineText
            );

            // --------------------------------------------------------
            // Move caret to duplicated line
            // --------------------------------------------------------

            EditorTextBox.CaretOffset =
                insertOffset +
                newline.Length +
                lineText.Length;
        }

        // ============================================================
        // FIND
        // ============================================================

        private void ShowFindPanel()
        {
            SearchPanel.Margin = new Thickness(0);

            SearchPanel.Visibility = Visibility.Visible;

            ReplaceRow.Visibility = Visibility.Collapsed;

            ReplaceButton.Visibility = Visibility.Collapsed;

            ReplaceAllButton.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(
                EditorTextBox.SelectedText
            ))
            {
                FindTextBox.Text = EditorTextBox.SelectedText;

                FindTextBox.SelectAll();
            }

            FindTextBox.Focus();
        }

        // ============================================================
        // REPLACE
        // ============================================================

        private void ShowReplacePanel()
        {
            SearchPanel.Margin = new Thickness(0);

            SearchPanel.Visibility = Visibility.Visible;

            ReplaceRow.Visibility = Visibility.Visible;

            ReplaceButton.Visibility = Visibility.Visible;

            ReplaceAllButton.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(
                EditorTextBox.SelectedText
            ))
            {
                FindTextBox.Text = EditorTextBox.SelectedText;

                FindTextBox.SelectAll();
            }

            FindTextBox.Focus();
        }

        // ============================================================
        // CLOSE
        // ============================================================

        private void CloseSearchPanel()
        {
            SearchPanel.Visibility = Visibility.Collapsed;

            EditorTextBox.Focus();
        }


        private void CloseSearchPanel_Click(
            object sender,
            RoutedEventArgs e)
        {
            CloseSearchPanel();
        }

        // ============================================================
        // SEARCH
        // ============================================================

        private bool FindNext()
        {
            string searchText = FindTextBox.Text;

            if (string.IsNullOrEmpty(
                searchText
            ))
            {
                return false;
            }

            string documentText = EditorTextBox.Text;


            int start = EditorTextBox.SelectionStart + EditorTextBox.SelectionLength;

            StringComparison comparison =
                MatchCaseCheckBox.IsChecked == true
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

            int index = documentText.IndexOf(
                searchText,
                start,
                comparison
            );

            if (index < 0)
            {
                index = documentText.IndexOf(
                    searchText,
                    0,
                    comparison
                );
            }

            if (index < 0)
                return false;

            EditorTextBox.Select(
                index,
                searchText.Length
            );

            EditorTextBox.ScrollToLine(
                EditorTextBox.Document
                    .GetLineByOffset(index)
                    .LineNumber
            );

            return true;
        }

        // ============================================================
        // FIND KEYBOARD
        // ============================================================

        private void SearchTextBox_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FindNext();

                e.Handled = true;

                return;
            }

            if (e.Key == Key.Escape)
            {
                CloseSearchPanel();

                e.Handled = true;
            }
        }

        // ============================================================
        // REPLACE KEYBOARD
        // ============================================================

        private void ReplaceTextBox_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReplaceButton_Click(
                    ReplaceButton,
                    new RoutedEventArgs()
                );

                e.Handled = true;

                return;
            }

            if (e.Key == Key.Escape)
            {
                CloseSearchPanel();

                e.Handled = true;
            }
        }

        // ============================================================
        // REPLACE ONE
        // ============================================================

        private void ReplaceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string find = FindTextBox.Text;

            string replace = ReplaceTextBox.Text;

            if (string.IsNullOrEmpty(find))
                return;

            StringComparison comparison =
                MatchCaseCheckBox.IsChecked == true
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

            bool selectedMatch = string.Equals(
                EditorTextBox.SelectedText,
                find,
                comparison
            );

            if (!selectedMatch)
            {
                FindNext();

                return;
            }

            EditorTextBox.Document.Replace(
                EditorTextBox.SelectionStart,
                EditorTextBox.SelectionLength,
                replace
            );

            FindNext();
        }

        // ============================================================
        // REPLACE ALL
        // ============================================================

        private void ReplaceAllButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string find = FindTextBox.Text;

            string replace = ReplaceTextBox.Text;

            if (string.IsNullOrEmpty(find))
                return;

            StringComparison comparison =
                MatchCaseCheckBox.IsChecked == true
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

            int position = 0;

            while (true)
            {
                string documentText = EditorTextBox.Text;

                int index = documentText.IndexOf(
                    find,
                    position,
                    comparison
                );

                if (index < 0)
                    break;

                EditorTextBox.Document.Replace(
                    index,
                    find.Length,
                    replace
                );

                position = index + replace.Length;
            }
        }

        // ============================================================
        // TEXT CHANGED
        // ============================================================

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