using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ArcanumTextureSlicer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                Console.WriteLine($"Not enough arguments.");
                return;
            }

            var inputFile = args[0];
            var outputFolder = args[1];

            const int tileWidth = 78;
            const int tileHeight = 40;

            try
            {
                using (var inputBitmap = new Bitmap(inputFile))
                {
                    var n = Math.Ceiling((double) inputBitmap.Width/tileWidth);
                    var m = Math.Ceiling((double) inputBitmap.Height/tileHeight);
                    for (var i = 0; i < n; i++)
                    {
                        for (var j = 0; j < m; j++)
                        {
                            using (var outputBitmap = CloneRegion(inputBitmap,
                                new Rectangle(i*tileWidth, j*tileHeight, tileWidth, tileHeight)))
                            {
                                try
                                {
                                    Console.WriteLine($"Save tile #{i},{j}.");
                                    outputBitmap.Save($"{outputFolder.TrimEnd('/', '\\')}\\tile_{i}_{j}.bmp",
                                        ImageFormat.Bmp);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"Error saving tile.");
                                }
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Invalid input file.");
            }
        }

        private static Bitmap CloneRegion(Bitmap source, Rectangle rect)
        {
            if (rect.X >= 0 && rect.Y >= 0 && rect.Right <= source.Width && rect.Bottom <= source.Height)
            {
                return source.Clone(rect, source.PixelFormat);
            }
            var b = new Bitmap(rect.Width, rect.Height, source.PixelFormat);
            // todo exception here
            using (var g = Graphics.FromImage(b))
            {
                using (var p = new Pen(Color.Blue))
                {
                    g.DrawRectangle(p, 0, 0, b.Width, b.Height);
                }
                g.DrawImage(source,
                    Math.Max(-rect.X, 0),
                    Math.Max(-rect.Y, 0),
                    Rectangle.Intersect(rect, new Rectangle(0, 0, source.Width, source.Height)),
                    GraphicsUnit.Pixel);
            }
            return null;
        }
    }
}