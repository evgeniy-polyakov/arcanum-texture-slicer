using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcanumTextureSlicer.Core;
using FontFamily = System.Drawing.FontFamily;
using Image = System.Windows.Controls.Image;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace ArcanumTextureSlicer.Gui.Controls
{
    public class GridViewer : Image
    {
        private const int GridTileWidth = Tile.Width + Tile.XSpace;
        private const int GridTileHeight = Tile.Height;
        private const uint ColorTransparent = 0x00000000;
        private const uint ColorSelection = 0x6600ff00;
        private const uint ColorGrid = 0xcc00ff00;
        private const uint ColorNumber = 0xff00ff00;
        private const int SizeNumber = 12;

        private readonly IList<uint[,]> _numberPixels = new List<uint[,]>();

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

        public void CreateGrid(Bitmap bitmap)
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
            UpdateSelectedIndices();

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
                            SetPixel(ColorSelection,
                                tile.X + OffsetX - Tile.Rows[r]/2 + p,
                                tile.Y + OffsetY - Tile.HalfHeight + r);
                        }
                    }

                    DrawNumber(tile.X + OffsetX, tile.Y + OffsetY, tile.SelectedIndex);
                }
                foreach (var point in Tile.Outline)
                {
                    SetPixel(ColorGrid,
                        tile.X + point.X + OffsetX,
                        tile.Y + point.Y + OffsetY);
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
                    if (tile.Selected)
                    {
                        tile.SelectedIndex = int.MaxValue;
                    }
                    break;
                }
            }
        }

        private void UpdateSelectedIndices()
        {
            var selectedTiles = _tiles.Where(t => t.Selected).OrderBy(t => t.SelectedIndex).ToList();
            for (var i = 0; i < selectedTiles.Count; i++)
            {
                selectedTiles[i].SelectedIndex = i + 1;
            }
        }

        private void ShiftSelectedTiles(int column, int row)
        {
            var selectedTiles = _tiles.Where(t => t.Selected).OrderBy(t => t.SelectedIndex).ToList();
            selectedTiles.ForEach(t => t.Selected = false);

            var i = 0;
            foreach (var tile in selectedTiles)
            {
                var newTile = _tiles.FirstOrDefault(t => t.Column == tile.Column + column && t.Row == tile.Row + row*2);
                if (newTile != null)
                {
                    newTile.Selected = true;
                    newTile.SelectedIndex = ++i;
                }
            }
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

        private void DrawNumber(int centerX, int centerY, int number)
        {
            while (_numberPixels.Count <= number)
            {
                _numberPixels.Add(NumberToPixels(_numberPixels.Count));
            }
            var pixels = _numberPixels[number];
            var n0 = pixels.GetLength(0);
            var n1 = pixels.GetLength(1);
            for (var ty = 0; ty < n0; ty++)
            {
                for (var tx = 0; tx < n1; tx++)
                {
                    if (pixels[ty, tx] > 0)
                    {
                        SetPixel(pixels[ty, tx],
                            centerX + tx - n1/2,
                            centerY + ty - n0/2);
                    }
                }
            }
        }

        private uint[,] NumberToPixels(int n)
        {
            var s = n.ToString("D");
            using (var bitmap = new Bitmap(2*SizeNumber*s.Length, 2*SizeNumber, PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    using (var font = new Font(
                        FontFamily.GenericSansSerif, SizeNumber, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        graphics.SmoothingMode = SmoothingMode.None;
                        graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                        var size = Size.Ceiling(graphics.MeasureString(s, font));
                        graphics.DrawString(s, font, new SolidBrush(ColorNumber.ToColor()), 0, 0);

                        var pixels = new uint[size.Height, size.Width];
                        for (var y = 0; y < size.Height; y++)
                        {
                            for (var x = 0; x < size.Width; x++)
                            {
                                pixels[y, x] = (uint) bitmap.GetPixel(x, y).ToArgb();
                            }
                        }
                        return pixels;
                    }
                }
            }
        }

        private void SetPixel(uint color, int x, int y)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                _pixels[y*_width + x] = color;
            }
        }
    }

    public class GridTile
    {
        public int Column;
        public int Row;
        public bool Selected;
        public int SelectedIndex;
        public int X;
        public int Y;
    }
}