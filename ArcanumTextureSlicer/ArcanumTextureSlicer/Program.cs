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
                using (var inputImage = Image.FromFile(inputFile, true))
                {
                    using (var inputBitmap = new Bitmap(inputImage))
                    {
                        for (int i = 0, n = inputBitmap.Width/tileWidth; i < n; i += 2)
                        {
                            for (int j = 0, m = inputBitmap.Height/tileHeight; j < m; j += 2)
                            {
                                using (
                                    var outputBitmap =
                                        inputBitmap.Clone(
                                            new Rectangle(i*tileWidth, j*tileHeight, tileWidth, tileHeight),
                                            PixelFormat.Format8bppIndexed))
                                {
                                    try
                                    {
                                        Console.WriteLine($"Save tile #{i},{j}.");
                                        outputBitmap.Save($"{outputFolder.TrimEnd('/', '\\')}\\tile_{i}_{j}");
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
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Invalid input file.");
            }
        }
    }
}