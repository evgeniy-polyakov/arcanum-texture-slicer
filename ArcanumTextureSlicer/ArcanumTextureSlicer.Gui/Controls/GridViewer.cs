using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcanumTextureSlicer.Core;
using Image = System.Windows.Controls.Image;

namespace ArcanumTextureSlicer.Gui.Controls
{
    public class GridViewer : Image
    {
        private int _offsetX;
        private int _offsetY;

        public int OffsetX
        {
            get { return _offsetX; }
            set
            {
                _offsetX = value;
                while (_offsetX < -Tile.Width - Tile.XSpace)
                {
                    _offsetX += Tile.Width + Tile.XSpace;
                }
                while (_offsetX > 0)
                {
                    _offsetX -= Tile.Width + Tile.XSpace;
                }
            }
        }

        public int OffsetY
        {
            get { return _offsetY; }
            set
            {
                _offsetY = value;
                while (_offsetY < -Tile.Height)
                {
                    _offsetY += Tile.Height;
                }
                while (_offsetY > 0)
                {
                    _offsetY -= Tile.Height;
                }
            }
        }

        public void DisplayGrid(Bitmap bitmap)
        {
            var stride = ((bitmap.Width*32 + 31) & ~31)/8;
            var pixels = new uint[bitmap.Width*bitmap.Height];

            foreach (var tile in bitmap.ToTiles(OffsetX, OffsetY))
            {
                foreach (var point in Tile.Outline)
                {
                    var x = tile.X + point.X;
                    var y = tile.Y + point.Y;
                    if (x >= 0 && x < bitmap.Width && y >= 0 && y < bitmap.Height)
                    {
                        pixels[y*bitmap.Width + x] = 0xcc00ff00;
                    }
                }
            }

            Source = BitmapSource.Create(
                bitmap.Width, bitmap.Height, 96, 96,
                PixelFormats.Bgra32, null, pixels, stride);
        }

        public void ClearSelection()
        {
        }

        public void SelectTileAt(int x, int y)
        {
        }
    }

    public struct GridTile
    {
        public int X;
        public int Y;
        public int Row;
        public int Column;
    }
}