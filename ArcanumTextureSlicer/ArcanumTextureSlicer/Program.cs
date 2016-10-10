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
                            using (var outputBitmap = inputBitmap.CloneRegion(
                                new Rectangle(i*tileWidth, j*tileHeight, tileWidth, tileHeight)))
                            {
                                try
                                {
                                    Console.WriteLine($"Save tile #{j},{i}.");
                                    outputBitmap.Save($"{outputFolder.TrimEnd('/', '\\')}\\tile_{j}_{i}.bmp",
                                        ImageFormat.Bmp);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}