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
        private const uint ColorTransparent = 0x00000000;
        private const uint ColorSelection = 0x6600ff00;
        private const uint ColorGrid = 0xcc00ff00;

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
                    ShiftSelectedTiles(-1, 0);
                }
                while (_offsetX > 0)
                {
                    _offsetX -= GridTileWidth;
                    ShiftSelectedTiles(1, 0);
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
                    ShiftSelectedTiles(0, -1);
                }
                while (_offsetY > 0)
                {
                    _offsetY -= GridTileHeight;
                    ShiftSelectedTiles(0, 1);
                }
            }
        }

        public void DisplayGrid(Bitmap bitmap)
        {
            _offsetX = 0;
            _offsetY = 0;

            _width = bitmap.Width;
            _height = bitmap.Height;

            _pixels = new uint[bitmap.Width*bitmap.Height];
            _stride = ((bitmap.Width*32 + 31) & ~31)/8;

            _tiles = Tile.Split(
                _width + GridTileWidth,
                _height + GridTileHeight)
                .Select(t => new GridTile
                {
                    X = t.X,
                    Y = t.Y,
                    Row = t.Row,
                    Column = t.Column
                })
                .ToList();

            UpdateGrid();
        }

        public void UpdateGrid()
        {
            CropSelectedTiles();

            for (var i = 0; i < _pixels.Length; i++)
            {
                _pixels[i] = ColorTransparent;
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
                                _pixels[y*_width + x] = ColorSelection;
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
                        _pixels[y*_width + x] = ColorGrid;
                    }
                }
                {
                    var x = tile.X + OffsetX;
                    var y = tile.Y + OffsetY;
                    if (x >= 0 && x < _width && y >= 0 && y < _height)
                    {
                        _pixels[y*_width + x] = ColorGrid;
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

        private void ShiftSelectedTiles(int column, int row)
        {
            var selectedTiles = _tiles.Where(t => t.Selected).ToList();
            selectedTiles.ForEach(t => t.Selected = false);

            selectedTiles.Select(t => _tiles.FirstOrDefault(
                t1 => t1.Column == t.Column + column && t1.Row == t.Row + row*2))
                .Where(t => t != null)
                .ToList()
                .ForEach(t => t.Selected = true);
        }

        private void CropSelectedTiles()
        {
            _tiles.Where(t =>
                t.X + Tile.HalfWidth + OffsetX < 0 ||
                t.X - Tile.HalfWidth + OffsetX >= _width ||
                t.Y + Tile.HalfHeight + OffsetY < 0 ||
                t.Y - Tile.HalfHeight + OffsetY >= _height)
                .ToList()
                .ForEach(t => t.Selected = false);
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