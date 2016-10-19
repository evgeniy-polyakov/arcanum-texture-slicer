using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcanumTextureSlicer.Core;
using Image = System.Windows.Controls.Image;

namespace ArcanumTextureSlicer.Gui.Controls
{
    public class GridViewer : Image
    {
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }

        public void DisplayGrid(Bitmap bitmap)
        {
            var stride = ((bitmap.Width*32 + 31) & ~31)/8;
            var pixels = new uint[bitmap.Width*bitmap.Height];

            bitmap.IterateTiles(-OffsetX, -OffsetY, position =>
            {
                foreach (var point in Tile.Outline)
                {
                    var x = position.X + point.X;
                    var y = position.Y + point.Y;
                    if (x >= 0 && x < bitmap.Width && y >= 0 && y < bitmap.Height)
                    {
                        pixels[y*bitmap.Width + x] = 0xcc00ff00;
                    }
                }
            });

            Source = BitmapSource.Create(
                bitmap.Width, bitmap.Height, 96, 96,
                PixelFormats.Bgra32, null, pixels, stride);
        }
    }
}