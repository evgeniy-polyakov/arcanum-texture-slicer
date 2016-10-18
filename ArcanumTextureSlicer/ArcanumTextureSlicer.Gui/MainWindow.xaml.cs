using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Color = System.Windows.Media.Color;
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
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
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
            var data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                ImageLockMode.ReadOnly, _bitmap.PixelFormat);
            try
            {
                //                var bytes = new byte[data.Height*data.Stride];
                //                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                //                var pixels = new uint[bytes.Length];
                //                var colors = _bitmap.Palette.Entries;
                //                for (var x = 0; x < data.Width; x++)
                //                {
                //                    for (var y = 0; y < data.Height; y++)
                //                    {
                //                        pixels[x + y*data.Stride] = (uint) colors[bytes[x + y*data.Stride]].ToArgb();
                //                    }
                //                }
                //                var stride = ((data.Width*32 + 31) & ~31)/8;
                //
                //                BitmapViewer.Source = BitmapSource.Create(
                //                    _bitmap.Width, _bitmap.Height, 96, 96,
                //                    PixelFormats.Bgra32, null, pixels, stride);

                var bytes = new byte[data.Height*data.Stride];
                var stride = data.Stride;
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                
                BitmapViewer.Source = BitmapSource.Create(
                    _bitmap.Width, _bitmap.Height, 96, 96, PixelFormats.Indexed8,
                    new BitmapPalette(_bitmap.Palette
                        .Entries
                        .Select(c => Color.FromArgb(c.A, c.R, c.G, c.B))
                        .ToList()),
                    bytes, stride);
            }
            catch (Exception e)
            {
                ShowError(e);
            }
            finally
            {
                _bitmap.UnlockBits(data);
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ShowError(Exception e)
        {
            MessageBox.Show(this, e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}