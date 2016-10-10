using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ArcanumTextureSlicer
{
    public static class BitmapExtensions
    {
        public static Bitmap CloneRegion(this Bitmap source, Rectangle rect)
        {
            if (rect.X >= 0 && rect.Y >= 0 && rect.Right <= source.Width && rect.Bottom <= source.Height)
            {
                return source.Clone(rect, source.PixelFormat);
            }
            var bitmap = source.Clone(new Rectangle(0, 0, rect.Width, rect.Height), source.PixelFormat);
            bitmap.SetColor(Color.Blue);
            bitmap.DrawImage(source,
                Math.Max(-rect.X, 0),
                Math.Max(-rect.Y, 0),
                Rectangle.Intersect(rect, new Rectangle(0, 0, source.Width, source.Height)));
            return bitmap;
        }

        public static void SetColor(this Bitmap canvas, Color color)
        {
            byte? colorIndex = null;
            var colors = canvas.Palette.Entries;
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

            var data = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height),
                ImageLockMode.WriteOnly, canvas.PixelFormat);
            var bytes = new byte[data.Height*data.Stride];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = colorIndex.Value;
            }

            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            canvas.UnlockBits(data);
        }

        public static void DrawImage(this Bitmap canvas, Bitmap source, int canvasX, int canvasY, Rectangle sourceRect)
        {
            var canvasData = canvas.LockBits(new Rectangle(canvasX, canvasY, sourceRect.Width, sourceRect.Height),
                ImageLockMode.WriteOnly, canvas.PixelFormat);
            var sourceData = source.LockBits(sourceRect, ImageLockMode.ReadOnly, canvas.PixelFormat);

            var canvasBytes = new byte[canvasData.Height*canvasData.Stride];
            var sourceBytes = new byte[sourceData.Height*sourceData.Stride];

            Marshal.Copy(sourceData.Scan0, sourceBytes, 0, sourceBytes.Length);

            for (var i = 0; i < canvasData.Width; i++)
            {
                for (var j = 0; j < canvasData.Height; j++)
                {
                    canvasBytes[i + j*canvasData.Stride] = sourceBytes[i + j*sourceData.Stride];
                }
            }

            Marshal.Copy(canvasBytes, 0, canvasData.Scan0, canvasBytes.Length);
            canvas.UnlockBits(canvasData);
            source.UnlockBits(sourceData);
        }
    }
}