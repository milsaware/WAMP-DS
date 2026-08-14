using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WAMP_DS.Models;

namespace WAMP_DS.Controls
{
    public partial class EditorTabs : UserControl
    {
        public event Action<OpenDocument>? DocumentSelected;

        public event Action<OpenDocument>? DocumentCloseRequested;

        public event Action<OpenDocument, int>? DocumentReordered;

        private OpenDocument? draggedDocument;

        private Point dragStartPoint;

        private Border? dropIndicator;


        public EditorTabs()
        {
            InitializeComponent();
        }


        public void SetDocuments(
            ObservableCollection<OpenDocument> documents)
        {
            HideDropIndicator();

            TabsItemsControl.Items.Clear();


            foreach (OpenDocument document in documents)
            {
                Border tab = CreateTab(document);

                TabsItemsControl.Items.Add(tab);
            }
        }


        private Border CreateTab(OpenDocument document)
        {
            Border tab = new()
            {
                Background = document.IsActive
                    ? new SolidColorBrush(Color.FromRgb(37, 37, 38))
                    : new SolidColorBrush(Color.FromRgb(45, 45, 48)),

                BorderBrush = document.IsActive
                    ? new SolidColorBrush(Color.FromRgb(0, 122, 204))
                    : new SolidColorBrush(Color.FromRgb(63, 63, 70)),

                BorderThickness = document.IsActive
                    ? new Thickness(0, 0, 0, 2)
                    : new Thickness(0, 0, 1, 0),

                Padding = new Thickness(10, 0, 5, 0),

                Height = 35,

                Tag = document,

                AllowDrop = true
            };


            Grid tabGrid = new();


            StackPanel panel = new()
            {
                Orientation = Orientation.Horizontal
            };


            Path icon = CreateFileIcon(
                document.FileName,
                document.IsActive
            );


            TextBlock name = new()
            {
                Foreground = document.IsActive
                    ? new SolidColorBrush(Color.FromRgb(255, 255, 255))
                    : new SolidColorBrush(Color.FromRgb(170, 170, 170)),

                VerticalAlignment = VerticalAlignment.Center
            };


            TextBlock close = new()
            {
                Text = "×",

                Foreground = new SolidColorBrush(
                    Color.FromRgb(170, 170, 170)
                ),

                FontSize = 16,

                Margin = new Thickness(10, 0, 0, 0),

                VerticalAlignment = VerticalAlignment.Center
            };


            Border indicator = new()
            {
                Width = 2,

                Background = new SolidColorBrush(
                    Color.FromRgb(0, 122, 204)
                ),

                HorizontalAlignment =
                    HorizontalAlignment.Left,

                VerticalAlignment =
                    VerticalAlignment.Stretch,

                Visibility =
                    Visibility.Collapsed,

                IsHitTestVisible = false
            };


            UpdateTabName(
                name,
                document
            );


            panel.Children.Add(icon);
            panel.Children.Add(name);
            panel.Children.Add(close);


            tabGrid.Children.Add(panel);
            tabGrid.Children.Add(indicator);


            tab.Child = tabGrid;


            tab.MouseLeftButtonDown += (sender, e) =>
            {
                if (e.OriginalSource == close)
                {
                    DocumentCloseRequested?.Invoke(
                        document
                    );

                    e.Handled = true;

                    return;
                }


                draggedDocument = document;

                dragStartPoint = e.GetPosition(tab);


                DocumentSelected?.Invoke(
                    document
                );
            };


            tab.MouseMove += (sender, e) =>
            {
                if (draggedDocument != document)
                    return;

                if (e.LeftButton != MouseButtonState.Pressed)
                    return;


                Point currentPosition =
                    e.GetPosition(tab);


                Vector difference =
                    currentPosition - dragStartPoint;


                if (Math.Abs(difference.X) <
                    SystemParameters.MinimumHorizontalDragDistance)
                {
                    return;
                }


                DragDrop.DoDragDrop(
                    tab,
                    document,
                    DragDropEffects.Move
                );


                draggedDocument = null;

                HideDropIndicator();
            };


            tab.DragOver += (sender, e) =>
            {
                if (!e.Data.GetDataPresent(
                    typeof(OpenDocument)))
                {
                    return;
                }


                OpenDocument? droppedDocument =
                    e.Data.GetData(
                        typeof(OpenDocument)
                    ) as OpenDocument;


                if (droppedDocument == null)
                    return;


                if (droppedDocument == document)
                {
                    HideDropIndicator();

                    e.Effects =
                        DragDropEffects.None;

                    e.Handled = true;

                    return;
                }


                Point position =
                    e.GetPosition(tab);


                bool insertBefore =
                    position.X <
                    tab.ActualWidth / 2;


                ShowDropIndicator(
                    indicator,
                    insertBefore
                );


                e.Effects =
                    DragDropEffects.Move;


                e.Handled = true;
            };


            tab.DragLeave += (sender, e) =>
            {
                HideDropIndicator();
            };


            tab.Drop += (sender, e) =>
            {
                if (!e.Data.GetDataPresent(
                    typeof(OpenDocument)))
                {
                    return;
                }


                OpenDocument? droppedDocument =
                    e.Data.GetData(
                        typeof(OpenDocument)
                    ) as OpenDocument;


                if (droppedDocument == null)
                    return;


                if (droppedDocument == document)
                    return;


                Point position =
                    e.GetPosition(tab);


                bool insertBefore =
                    position.X <
                    tab.ActualWidth / 2;


                int targetIndex =
                    TabsItemsControl.Items.IndexOf(
                        tab
                    );


                if (!insertBefore)
                    targetIndex++;


                DocumentReordered?.Invoke(
                    droppedDocument,
                    targetIndex
                );


                HideDropIndicator();


                e.Handled = true;
            };


            document.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName ==
                        nameof(OpenDocument.IsModified) ||
                    e.PropertyName ==
                        nameof(OpenDocument.IsActive))
                {
                    UpdateTabAppearance(
                        tab,
                        icon,
                        name,
                        document
                    );
                }
            };


            return tab;
        }


        private void ShowDropIndicator(
            Border indicator,
            bool insertBefore)
        {
            HideDropIndicator();


            dropIndicator = indicator;


            indicator.HorizontalAlignment =
                insertBefore
                    ? HorizontalAlignment.Left
                    : HorizontalAlignment.Right;


            indicator.Visibility =
                Visibility.Visible;
        }


        private void HideDropIndicator()
        {
            if (dropIndicator == null)
                return;


            dropIndicator.Visibility =
                Visibility.Collapsed;


            dropIndicator = null;
        }


        private static void UpdateTabAppearance(
            Border tab,
            Path icon,
            TextBlock name,
            OpenDocument document)
        {
            tab.Background = document.IsActive
                ? new SolidColorBrush(Color.FromRgb(37, 37, 38))
                : new SolidColorBrush(Color.FromRgb(45, 45, 48));


            tab.BorderBrush = document.IsActive
                ? new SolidColorBrush(Color.FromRgb(0, 122, 204))
                : new SolidColorBrush(Color.FromRgb(63, 63, 70));


            tab.BorderThickness = document.IsActive
                ? new Thickness(0, 0, 0, 2)
                : new Thickness(0, 0, 1, 0);


            name.Foreground = document.IsActive
                ? new SolidColorBrush(Color.FromRgb(255, 255, 255))
                : new SolidColorBrush(Color.FromRgb(170, 170, 170));


            icon.Fill = GetFileIconBrush(
                document.FileName,
                document.IsActive
            );


            UpdateTabName(
                name,
                document
            );
        }


        private static void UpdateTabName(
            TextBlock name,
            OpenDocument document)
        {
            name.Text = document.IsModified
                ? $"{document.FileName} *"
                : document.FileName;
        }


        private static Path CreateFileIcon(
            string fileName,
            bool isActive)
        {
            return new Path
            {
                Data = GetFileIconGeometry(
                    fileName
                ),

                Fill = GetFileIconBrush(
                    fileName,
                    isActive
                ),

                Width = 14,

                Height = 14,

                Stretch = Stretch.Uniform,

                Margin = new Thickness(
                    0,
                    0,
                    6,
                    0
                ),

                VerticalAlignment =
                    VerticalAlignment.Center
            };
        }


        private static Brush GetFileIconBrush(
            string fileName,
            bool isActive)
        {
            string extension = System.IO.Path
                .GetExtension(fileName)
                .ToLower();


            return extension switch
            {
                ".php" => new SolidColorBrush(
                    Color.FromRgb(119, 119, 119)
                ),

                ".html" => new SolidColorBrush(
                    Color.FromRgb(227, 79, 38)
                ),

                ".htm" => new SolidColorBrush(
                    Color.FromRgb(227, 79, 38)
                ),

                ".css" => new SolidColorBrush(
                    Color.FromRgb(38, 77, 228)
                ),

                ".js" => new SolidColorBrush(
                    Color.FromRgb(240, 219, 79)
                ),

                ".json" => new SolidColorBrush(
                    Color.FromRgb(160, 160, 160)
                ),

                ".xml" => new SolidColorBrush(
                    Color.FromRgb(160, 160, 160)
                ),

                ".cs" => new SolidColorBrush(
                    Color.FromRgb(104, 33, 122)
                ),

                ".sql" => new SolidColorBrush(
                    Color.FromRgb(160, 160, 160)
                ),

                ".txt" => new SolidColorBrush(
                    Color.FromRgb(190, 190, 190)
                ),

                _ => new SolidColorBrush(
                    Color.FromRgb(190, 190, 190)
                )
            };
        }


        private static Geometry GetFileIconGeometry(
            string fileName)
        {
            string extension = System.IO.Path
                .GetExtension(fileName)
                .ToLower();


            return extension switch
            {
                ".php" => Geometry.Parse(
                    "M2,2 L12,2 L12,12 L2,12 Z"
                ),

                ".html" => Geometry.Parse(
                    "M2,2 L12,2 L11,12 L3,12 Z"
                ),

                ".htm" => Geometry.Parse(
                    "M2,2 L12,2 L11,12 L3,12 Z"
                ),

                ".css" => Geometry.Parse(
                    "M2,2 L12,2 L10,12 L4,12 Z"
                ),

                ".js" => Geometry.Parse(
                    "M2,2 L12,2 L12,12 L2,12 Z"
                ),

                ".json" => Geometry.Parse(
                    "M3,2 L11,2 L11,12 L3,12 Z"
                ),

                ".xml" => Geometry.Parse(
                    "M2,3 L5,7 L2,11 M12,3 L9,7 L12,11"
                ),

                ".cs" => Geometry.Parse(
                    "M7,1 L13,7 L7,13 L1,7 Z"
                ),

                ".sql" => Geometry.Parse(
                    "M2,3 C2,1 12,1 12,3 C12,5 2,5 2,3 M2,3 L2,10 C2,12 12,12 12,10 L12,3"
                ),

                ".txt" => Geometry.Parse(
                    "M3,2 L11,2 L11,12 L3,12 Z"
                ),

                _ => Geometry.Parse(
                    "M3,2 L11,2 L11,12 L3,12 Z"
                )
            };
        }
    }
}