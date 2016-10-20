using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcanumTextureSlicer.Core;
using Image = System.Windows.Controls.Image;

namespace ArcanumTextureSlicer.Gui.Controls
{
    public class GridViewer : Image
    {
        private const int GridTileWidth = Tile.Width + Tile.XSpace;
        private const int GridTileHeight = Tile.Height;
        private int _height;
        private int _offsetX;
        private int _offsetY;
        private uint[] _pixels;
        private int _stride;
        private IList<GridTile> _tiles;

        private int _width;

        public int OffsetX
        {
            get { return _offsetX; }
            set
            {
                _offsetX = value;
                while (_offsetX < -GridTileWidth)
                {
                    _offsetX += GridTileWidth;
                }
                while (_offsetX > 0)
                {
                    _offsetX -= GridTileWidth;
                }
            }
        }

        public int OffsetY
        {
            get { return _offsetY; }
            set
            {
                _offsetY = value;
                while (_offsetY < -GridTileHeight)
                {
                    _offsetY += GridTileHeight;
                }
                while (_offsetY > 0)
                {
                    _offsetY -= GridTileHeight;
                }
            }
        }

        public void DisplayGrid(Bitmap bitmap)
        {
            _width = bitmap.Width;
            _height = bitmap.Height;

            _pixels = new uint[bitmap.Width*bitmap.Height];
            _stride = ((bitmap.Width*32 + 31) & ~31)/8;

            _tiles = bitmap
                .ToTiles(-GridTileWidth, -GridTileHeight)
                .Select(t => new GridTile
                {
                    X = t.X + GridTileWidth,
                    Y = t.Y + GridTileHeight,
                    Row = t.Row,
                    Column = t.Column
                })
                .ToList();

            UpdateGrid();
        }

        public void UpdateGrid()
        {
            for (var i = 0; i < _pixels.Length; i++)
            {
                _pixels[i] = 0x00000000;
            }
            foreach (var tile in _tiles)
            {
                foreach (var point in Tile.Outline)
                {
                    var x = tile.X + point.X + OffsetX;
                    var y = tile.Y + point.Y + OffsetY;
                    if (x >= 0 && x < _width && y >= 0 && y < _height)
                    {
                        _pixels[y*_width + x] = 0xcc00ff00;
                    }
                }
            }
            Source = BitmapSource.Create(
                _width, _height, 96, 96,
                PixelFormats.Bgra32, null, _pixels, _stride);
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
        public bool Selected;
    }
}