using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ArcanumTextureSlicer.Gui.Commands;
using Microsoft.Win32;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;

namespace ArcanumTextureSlicer.Gui
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap _bitmap;
        private Point _mousePosition;
        private Point _scrollOffset;
        private double _zoom = 1.0;

        public MainWindow()
        {
            InitializeComponent();

            RenderOptions.SetBitmapScalingMode(BitmapViewer, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(GridViewer, BitmapScalingMode.NearestNeighbor);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                OpenFile(args[1]);
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                ShowReadOnly = true,
                Filter = "Bitmap Images (*.bmp)|*.bmp|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                OpenFile(openFileDialog.FileName);
            }
        }

        private void OpenFile(string file)
        {
            var bitmap = CreateBitmap(file);
            if (bitmap != null)
            {
                DestroyBitmap();
                _bitmap = bitmap;
                DisplayBitmap();
            }
        }

        private Bitmap CreateBitmap(string file)
        {
            Bitmap bitmap = null;
            try
            {
                bitmap = new Bitmap(file);
                if (!bitmap.RawFormat.Equals(ImageFormat.Bmp)
                    || !bitmap.PixelFormat.Equals(PixelFormat.Format8bppIndexed))
                {
                    throw new Exception("Incorrect image format. 8bit bmp is expected.");
                }
                return bitmap;
            }
            catch (Exception e)
            {
                bitmap?.Dispose();
                ShowError(e);
            }
            return null;
        }

        private void DestroyBitmap()
        {
            if (_bitmap != null)
            {
                BitmapViewer.Source = null;
                _bitmap.Dispose();
            }
        }

        private void DisplayBitmap()
        {
            try
            {
                _zoom = 1.0;
                UpdateZoom();
                BitmapViewer.DisplayBitmap(_bitmap);
                GridViewer.CreateGrid(_bitmap);
            }
            catch (Exception e)
            {
                ShowError(e);
            }
        }

        private void ShowError(Exception e)
        {
            MessageBox.Show(this, e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MoveGrid_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _bitmap != null;
        }

        private void MoveGrid_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var moveGridCommand = e.Command as MoveGridCommand;
            if (moveGridCommand != null)
            {
                GridViewer.OffsetX += moveGridCommand.X;
                GridViewer.OffsetY += moveGridCommand.Y;
            }
            var resetGridCommand = e.Command as ResetGridCommand;
            if (resetGridCommand != null)
            {
                GridViewer.OffsetX = 0;
                GridViewer.OffsetY = 0;
                GridViewer.ClearSelection();
            }
            GridViewer.UpdateGrid();
        }

        private void GridViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_bitmap != null && (Keyboard.Modifiers & ModifierKeys.Control) > 0)
            {
                var p = Mouse.GetPosition(GridViewer);
                var scale = _bitmap.Width/GridViewer.ActualWidth;
                p.X *= scale;
                p.Y *= scale;
                GridViewer.SelectTileAt((int) p.X, (int) p.Y);
                GridViewer.UpdateGrid();
            }
        }

        private void Zoom_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _bitmap != null;
        }

        private void Zoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var zoomCommnand = (ZoomCommand) e.Command;
            _zoom = zoomCommnand.Zoom;
            UpdateZoom();
        }

        private void ScrollContent_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_bitmap != null && (Keyboard.Modifiers & ModifierKeys.Control) > 0)
            {
                _zoom += 0.001*e.Delta;
                UpdateZoom();
                e.Handled = true;
            }
        }

        private void UpdateZoom()
        {
            _zoom = Math.Max(1, _zoom);
            _zoom = Math.Min(4, _zoom);

            if (ScrollViewer.ScrollableHeight > 0 && ScrollViewer.ScrollableWidth > 0)
            {
                var verticalScrollPosition = ScrollViewer.VerticalOffset/ScrollViewer.ScrollableHeight;
                var horizontalScrollPosition = ScrollViewer.HorizontalOffset/ScrollViewer.ScrollableWidth;

                ScrollContent.LayoutTransform = new ScaleTransform(_zoom, _zoom);
                ScrollViewer.UpdateLayout();

                ScrollViewer.ScrollToVerticalOffset(verticalScrollPosition*ScrollViewer.ScrollableHeight);
                ScrollViewer.ScrollToHorizontalOffset(horizontalScrollPosition*ScrollViewer.ScrollableWidth);
            }
            else
            {
                ScrollContent.LayoutTransform = new ScaleTransform(_zoom, _zoom);
            }
        }

        private void ScrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_bitmap != null && (Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                _mousePosition = e.GetPosition(ScrollViewer);
                _scrollOffset = new Point(ScrollViewer.HorizontalOffset, ScrollViewer.VerticalOffset);
                ScrollViewer.CaptureMouse();
            }
        }

        private void ScrollViewer_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (ScrollViewer.IsMouseCaptured)
            {
                var p = e.GetPosition(ScrollViewer);
                ScrollViewer.ScrollToVerticalOffset(_scrollOffset.Y + _mousePosition.Y - p.Y);
                ScrollViewer.ScrollToHorizontalOffset(_scrollOffset.X + _mousePosition.X - p.X);
            }
        }

        private void ScrollViewer_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ScrollViewer.IsMouseCaptured)
            {
                ScrollViewer.ReleaseMouseCapture();
            }
        }
    }
}