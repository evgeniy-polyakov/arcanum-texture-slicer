using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
                if (tile.Selected)
                {
                    for (var r = 0; r < Tile.Rows.Length; r++)
                    {
                        for (var p = 0; p < Tile.Rows[r]; p++)
                        {
                            var x = tile.X + OffsetX - Tile.Rows[r]/2 + p;
                            var y = tile.Y + OffsetY - Tile.HalfHeight + r;
                            if (x >= 0 && x < _width && y >= 0 && y < _height)
                            {
                                _pixels[y*_width + x] = 0x7f00ff00;
                            }
                        }
                    }
                }
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
            foreach (var tile in _tiles)
            {
                tile.Selected = false;
            }
        }

        public void SelectTileAt(int x, int y)
        {
            foreach (var tile in _tiles)
            {
                if (Tile.HitTest(x - tile.X - OffsetX, y - tile.Y - OffsetY))
                {
                    tile.Selected = !tile.Selected;
                    UpdateGrid();
                    break;
                }
            }
        }
    }

    public class GridTile
    {
        public int Column;
        public int Row;
        public bool Selected;
        public int X;
        public int Y;
    }
}