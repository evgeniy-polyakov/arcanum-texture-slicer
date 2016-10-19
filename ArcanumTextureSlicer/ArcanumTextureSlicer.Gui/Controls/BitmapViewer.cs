using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

namespace ArcanumTextureSlicer.Gui.Controls
{
    public class BitmapViewer : Image
    {
        public void DisplayBitmap(Bitmap bitmap)
        {
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            try
            {
                var bytes = new byte[data.Height*data.Stride];
                var stride = data.Stride;
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

                Source = BitmapSource.Create(
                    bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Indexed8,
                    new BitmapPalette(bitmap.Palette
                        .Entries
                        .Select(c => Color.FromArgb(c.A, c.R, c.G, c.B))
                        .ToList()),
                    bytes, stride);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
    }
}