using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ArcanumTextureSlicer.Core
{
    public static class BitmapExtensions
    {
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
            var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height),
                ImageLockMode.ReadOnly, source.PixelFormat);
            try
            {
                var sourceBytes = new byte[sourceData.Height*sourceData.Stride];
                Marshal.Copy(sourceData.Scan0, sourceBytes, 0, sourceBytes.Length);

                for (var y = 0; y < sourceData.Height - Tile.HalfHeight + 1; y++)
                {
                    for (var x = Tile.HalfWidth - 1; x < sourceData.Width - Tile.HalfWidth; x++)
                    {
                        var index = y*sourceData.Stride + x;
                        if (sourceBytes[index] == colorIndex)
                        {
                            for (var r = 0; r < Tile.Rows.Length; r++)
                            {
                                for (var p = 0; p < Tile.Rows[r]; p++)
                                {
                                    var i = index + p + r*sourceData.Stride - (Tile.Rows[r] - 2)/2;
                                    if (sourceBytes[i] != colorIndex)
                                    {
                                        goto Continue;
                                    }
                                }
                            }
                            point.X = x + 1;
                            point.Y = y + Tile.HalfHeight;
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

        public static Bitmap CreateTile(this Bitmap source, int x, int y)
        {
            var tile = CloneRegion(source, new Rectangle(x, y, Tile.Width, Tile.Height));
            tile.DrawAlpha();
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

        public static void DrawAlpha(this Bitmap tile)
        {
            var rect = new Rectangle(0, 0, tile.Width, tile.Height);
            var data = tile.LockBits(rect, ImageLockMode.WriteOnly, tile.PixelFormat);
            try
            {
                var canvasBytes = new byte[data.Height*data.Stride];
                Marshal.Copy(data.Scan0, canvasBytes, 0, canvasBytes.Length);

                for (var y = 0; y < Tile.Height; y++)
                {
                    for (var x = 0; x < Tile.Width; x++)
                    {
                        if (!Tile.HitTest(x, y))
                        {
                            canvasBytes[x + y*data.Stride] = 0;
                        }
                    }
                }

                Marshal.Copy(canvasBytes, 0, data.Scan0, canvasBytes.Length);
            }
            finally
            {
                tile.UnlockBits(data);
            }
        }

        public static void DrawImage(this Bitmap canvas, Bitmap source, int canvasX, int canvasY, Rectangle sourceRect)
        {
            if (sourceRect.Width == 0 || sourceRect.Height == 0)
            {
                return;
            }
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

        public static IEnumerable<Tile> ToTiles(this Bitmap bitmap, int initTileX, int initTileY)
        {
            var n = Math.Ceiling((double) (bitmap.Width - initTileX)/(Tile.Width + Tile.XSpace)) + 1;
            var m = Math.Ceiling((double) (bitmap.Height - initTileY)/Tile.Height)*2 + 2;
            for (var j = 0; j < m; j++)
            {
                for (var i = 0; i < n; i++)
                {
                    var evenRow = j%2;
                    var oddRow = 1 - evenRow;

                    var tileX = initTileX + i*Tile.Width - Tile.HalfWidth*oddRow + i*Tile.XSpace +
                                Tile.HalfXSpace*evenRow;
                    var tileY = initTileY + j*Tile.HalfHeight - Tile.HalfHeight;

                    yield return new Tile
                    {
                        Row = j,
                        Column = i,
                        X = tileX,
                        Y = tileY
                    };
                }
            }
        }
    }
}