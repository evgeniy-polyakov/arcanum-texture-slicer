using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ArcanumTextureSlicer.Gui.Commands;
using Microsoft.Win32;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace ArcanumTextureSlicer.Gui
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap _bitmap;

        public MainWindow()
        {
            InitializeComponent();

            RenderOptions.SetBitmapScalingMode(BitmapViewer, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(GridViewer, BitmapScalingMode.NearestNeighbor);
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
                var bitmap = CreateBitmap(openFileDialog.FileName);
                if (bitmap != null)
                {
                    DestroyBitmap();
                    _bitmap = bitmap;
                    DisplayBitmap();
                }
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
                BitmapViewer.DisplayBitmap(_bitmap);
                GridViewer.DisplayGrid(_bitmap);
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
            }
            GridViewer.UpdateGrid();
        }
    }
}