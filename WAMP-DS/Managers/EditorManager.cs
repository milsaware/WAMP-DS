using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using WAMP_DS.Controls;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class EditorManager
    {
        private readonly CodeEditor editor;

        private bool loadingDocument;

        private int untitledDocumentCount = 0;

        public ObservableCollection<OpenDocument> OpenDocuments { get; } = new();

        public OpenDocument? ActiveDocument { get; private set; }

        public EditorManager(CodeEditor editor)
        {
            this.editor = editor;

            editor.TextChanged += Editor_TextChanged;
        }

        public void CreateNewDocument(
    string extension = "",
    string content = "")
        {
            untitledDocumentCount++;

            OpenDocument newDocument = new()
            {
                FilePath = string.Empty,
                FileName = $"Untitled-{untitledDocumentCount}{extension}",
                Content = content,
                IsModified = false,
                IsActive = true
            };

            OpenDocuments.Add(newDocument);

            SetActiveDocument(newDocument);
        }

        public void OpenFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            string fullFilePath;

            try
            {
                fullFilePath = Path.GetFullPath(filePath);
            }
            catch
            {
                return;
            }

            OpenDocument? existingDocument = null;

            foreach (OpenDocument document in OpenDocuments)
            {
                if (string.IsNullOrEmpty(document.FilePath))
                    continue;

                string existingFilePath;

                try
                {
                    existingFilePath = Path.GetFullPath(
                        document.FilePath
                    );
                }
                catch
                {
                    continue;
                }

                if (existingFilePath.Equals(
                    fullFilePath,
                    StringComparison.OrdinalIgnoreCase))
                {
                    existingDocument = document;

                    break;
                }
            }

            if (existingDocument != null)
            {
                SetActiveDocument(existingDocument);

                return;
            }

            string content;

            try
            {
                content = File.ReadAllText(fullFilePath);
            }
            catch
            {
                return;
            }

            OpenDocument newDocument = new()
            {
                FilePath = fullFilePath,
                FileName = Path.GetFileName(fullFilePath),
                Content = content,
                IsModified = false,
                IsActive = true
            };

            OpenDocuments.Add(newDocument);

            SetActiveDocument(newDocument);
        }

        public bool SaveActiveDocument()
        {
            if (ActiveDocument == null)
                return false;

            return SaveDocument(
                ActiveDocument,
                out _
            );
        }

        public bool SaveActiveDocumentAs(
            out string? errorMessage)
        {
            errorMessage = null;

            if (ActiveDocument == null)
            {
                errorMessage = "There is no active document.";

                return false;
            }

            return SaveDocumentAs(
                ActiveDocument,
                out errorMessage
            );
        }

        public bool SaveDocumentAs(
            OpenDocument document,
            out string? errorMessage)
        {
            errorMessage = null;

            if (!OpenDocuments.Contains(document))
            {
                errorMessage =
                    "The document is not currently open.";

                return false;
            }

            SaveFileDialog dialog = new()
            {
                Title = "Save File As",
                FileName = document.FileName,
                Filter = "All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true)
                return false;

            string newFilePath;

            try
            {
                newFilePath = Path.GetFullPath(
                    dialog.FileName
                );
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return false;
            }

            foreach (OpenDocument openDocument in OpenDocuments)
            {
                if (openDocument == document)
                    continue;

                if (string.IsNullOrEmpty(openDocument.FilePath))
                    continue;

                string existingFilePath;

                try
                {
                    existingFilePath = Path.GetFullPath(
                        openDocument.FilePath
                    );
                }
                catch
                {
                    continue;
                }

                if (!existingFilePath.Equals(
                    newFilePath,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                errorMessage =
                    $"The file \"{openDocument.FileName}\" is already open.";

                return false;
            }

            try
            {
                File.WriteAllText(
                    newFilePath,
                    document.Content
                );

                document.FilePath = newFilePath;

                document.FileName =
                    Path.GetFileName(newFilePath);

                document.IsModified = false;

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return false;
            }
        }

        public void SetActiveDocument(OpenDocument document)
        {
            if (!OpenDocuments.Contains(document))
                return;

            foreach (OpenDocument openDocument in OpenDocuments)
            {
                openDocument.IsActive = false;
            }

            document.IsActive = true;

            ActiveDocument = document;

            loadingDocument = true;

            try
            {
                editor.SetLanguage(
                    Path.GetExtension(document.FileName)
                );

                editor.Text = document.Content;
            }
            finally
            {
                loadingDocument = false;
            }
        }

        private void Editor_TextChanged(
            object? sender,
            EventArgs e)
        {
            if (loadingDocument)
                return;

            if (ActiveDocument == null)
                return;

            ActiveDocument.Content = editor.Text;

            ActiveDocument.IsModified = true;
        }

        public void CloseDocument(OpenDocument document)
        {
            if (!OpenDocuments.Contains(document))
                return;

            bool wasActive = document == ActiveDocument;

            OpenDocuments.Remove(document);

            if (!wasActive)
                return;

            if (OpenDocuments.Count == 0)
            {
                ActiveDocument = null;

                loadingDocument = true;

                try
                {
                    editor.Text = string.Empty;
                }
                finally
                {
                    loadingDocument = false;
                }

                return;
            }

            SetActiveDocument(
                OpenDocuments[^1]
            );
        }

        public bool SaveDocument(
            OpenDocument document,
            out string? errorMessage)
        {
            errorMessage = null;

            if (!OpenDocuments.Contains(document))
            {
                errorMessage =
                    "The document is not currently open.";

                return false;
            }

            if (string.IsNullOrEmpty(document.FilePath))
            {
                return SaveDocumentAs(
                    document,
                    out errorMessage
                );
            }

            string fullFilePath;

            try
            {
                fullFilePath = Path.GetFullPath(
                    document.FilePath
                );
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return false;
            }

            try
            {
                File.WriteAllText(
                    fullFilePath,
                    document.Content
                );

                document.IsModified = false;

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return false;
            }
        }
    }
}