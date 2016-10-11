using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ArcanumTextureSlicer
{
    public static class BitmapExtensions
    {
        public const int TileWidth = 78;
        public const int HalfTileWidth = 39;
        public const int TileHeight = 40;
        public const int HalfTileHeight = 20;
        public const int TileXSpace = 2;
        public const int HalfTileXSpace = 1;
        private static Bitmap _sampleTile;

        public static Bitmap SampleTile =>
            _sampleTile ??
            (_sampleTile = new Bitmap(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("ArcanumTextureSlicer.Resources.SampleTile.png")));

        public static Point GetStartTileCenter(this Bitmap source)
        {
            var point = new Point();
            byte colorIndex;
            try
            {
                colorIndex = source.GetColorIndex(Color.Black);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"Start tile color {Color.Black} is not found in palette.");
                return point;
            }
            var tileRows = GetTileRows();
            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height),
                ImageLockMode.ReadOnly, source.PixelFormat);
            try
            {
                var sourceBytes = new byte[sourceData.Height*sourceData.Stride];
                Marshal.Copy(sourceData.Scan0, sourceBytes, 0, sourceBytes.Length);

                for (var y = 0; y < sourceData.Height - HalfTileHeight + 1; y++)
                {
                    for (var x = HalfTileWidth - 1; x < sourceData.Width - HalfTileWidth; x++)
                    {
                        var index = y*sourceData.Stride + x;
                        if (sourceBytes[index] == colorIndex)
                        {
                            for (var r = 0; r < tileRows.Length; r++)
                            {
                                for (var p = 0; p < tileRows[r]; p++)
                                {
                                    var i = index + p + r*sourceData.Stride - (tileRows[r] - 2)/2;
                                    if (sourceBytes[i] != colorIndex)
                                    {
                                        goto Continue;
                                    }
                                }
                            }
                            point.X = x + 1;
                            point.Y = y + HalfTileHeight;
                            goto Finish;
                        }
                        Continue:
                        ;
                    }
                }
                Finish:
                ;
            }
            finally
            {
                source.UnlockBits(sourceData);
            }
            Console.WriteLine($"Start tile is not found.");
            return point;
        }

        private static int[] GetTileRows()
        {
            var rows = new int[40];
            for (var i = 0; i < 20; i++)
            {
                rows[i] = 2 + i*4;
            }
            for (var i = 19; i >= 0; i--)
            {
                rows[39 - i] = 2 + i*4;
            }
            return rows;
        }

        //private static bool 

        public static Bitmap CreateTile(this Bitmap source, int x, int y)
        {
            var tile = CloneRegion(source, new Rectangle(x, y, SampleTile.Width, SampleTile.Height));
            tile.DrawAlpha(SampleTile);
            return tile;
        }

        public static Bitmap CloneRegion(this Bitmap source, Rectangle rect)
        {
            if (rect.X >= 0 && rect.Y >= 0 && rect.Right <= source.Width && rect.Bottom <= source.Height)
            {
                return source.Clone(rect, source.PixelFormat);
            }
            var bitmap = source.Clone(new Rectangle(0, 0, rect.Width, rect.Height), source.PixelFormat);
            bitmap.SetColor(0);
            bitmap.DrawImage(source,
                Math.Max(-rect.X, 0),
                Math.Max(-rect.Y, 0),
                Rectangle.Intersect(rect, new Rectangle(0, 0, source.Width, source.Height)));
            return bitmap;
        }

        public static void SetColor(this Bitmap canvas, Color color)
        {
            SetColor(canvas, canvas.GetColorIndex(color));
        }

        public static void SetColor(this Bitmap canvas, byte colorIndex)
        {
            var data = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height),
                ImageLockMode.WriteOnly, canvas.PixelFormat);
            try
            {
                var bytes = new byte[data.Height*data.Stride];

                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = colorIndex;
                }
                Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            }
            finally
            {
                canvas.UnlockBits(data);
            }
        }

        public static void DrawAlpha(this Bitmap canvas, Bitmap sample)
        {
            var rect = new Rectangle(0, 0, canvas.Width, canvas.Height);

            var sampleData = sample.LockBits(rect, ImageLockMode.ReadOnly, sample.PixelFormat);
            var canvasData = canvas.LockBits(rect, ImageLockMode.WriteOnly, canvas.PixelFormat);
            try
            {
                var sampleAlphaIndex = sample.GetColorIndex(Color.Blue);

                var sampleBytes = new byte[sampleData.Height*sampleData.Stride];
                var canvasBytes = new byte[canvasData.Height*canvasData.Stride];
                Marshal.Copy(sampleData.Scan0, sampleBytes, 0, sampleBytes.Length);
                Marshal.Copy(canvasData.Scan0, canvasBytes, 0, canvasBytes.Length);

                for (var y = 0; y < canvasData.Height; y++)
                {
                    for (var x = 0; x < canvasData.Width; x++)
                    {
                        if (sampleBytes[x + y*sampleData.Stride] == sampleAlphaIndex)
                        {
                            canvasBytes[x + y*canvasData.Stride] = 0;
                        }
                    }
                }

                Marshal.Copy(canvasBytes, 0, canvasData.Scan0, canvasBytes.Length);
            }
            finally
            {
                sample.UnlockBits(sampleData);
                canvas.UnlockBits(canvasData);
            }
        }

        public static void DrawImage(this Bitmap canvas, Bitmap source, int canvasX, int canvasY, Rectangle sourceRect)
        {
            var sourceData = source.LockBits(sourceRect, ImageLockMode.ReadOnly, source.PixelFormat);
            var canvasData = canvas.LockBits(new Rectangle(canvasX, canvasY, sourceRect.Width, sourceRect.Height),
                ImageLockMode.WriteOnly, canvas.PixelFormat);
            try
            {
                var sourceBytes = new byte[sourceData.Height*sourceData.Stride];
                var canvasBytes = new byte[canvasData.Height*canvasData.Stride];
                Marshal.Copy(sourceData.Scan0, sourceBytes, 0, sourceBytes.Length);
                Marshal.Copy(canvasData.Scan0, canvasBytes, 0, canvasBytes.Length);

                for (var y = 0; y < canvasData.Height; y++)
                {
                    for (var x = 0; x < canvasData.Width; x++)
                    {
                        canvasBytes[x + y*canvasData.Stride] = sourceBytes[x + y*sourceData.Stride];
                    }
                }

                Marshal.Copy(canvasBytes, 0, canvasData.Scan0, canvasBytes.Length);
            }
            finally
            {
                source.UnlockBits(sourceData);
                canvas.UnlockBits(canvasData);
            }
        }

        public static byte GetColorIndex(this Bitmap bitmap, Color color)
        {
            byte? colorIndex = null;
            var colors = bitmap.Palette.Entries;
            for (var i = 0; i < colors.Length; i++)
            {
                if (colors[i].ToArgb() == color.ToArgb())
                {
                    colorIndex = (byte) i;
                    break;
                }
            }
            if (!colorIndex.HasValue)
            {
                throw new ArgumentException($"No color {color} in palette.");
            }
            return colorIndex.Value;
        }

        public static bool IsTransparent(this Bitmap canvas)
        {
            var canvasData = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height),
                ImageLockMode.ReadOnly, canvas.PixelFormat);
            try
            {
                var canvasBytes = new byte[canvasData.Height*canvasData.Stride];
                Marshal.Copy(canvasData.Scan0, canvasBytes, 0, canvasBytes.Length);

                for (var y = 0; y < canvasData.Height; y++)
                {
                    for (var x = 0; x < canvasData.Width; x++)
                    {
                        if (canvasBytes[x + y*canvasData.Stride] != canvasBytes[0])
                        {
                            return false;
                        }
                    }
                }
            }
            finally
            {
                canvas.UnlockBits(canvasData);
            }
            return true;
        }
    }
}