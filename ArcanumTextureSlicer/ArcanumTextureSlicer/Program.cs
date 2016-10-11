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
            var initTileX = args.Length > 2 ? int.Parse(args[2]) : 0;
            var initTileY = args.Length > 3 ? int.Parse(args[3]) : 0;

            var tileWidth = BitmapExtensions.SampleTile.Width;
            var tileHeight = BitmapExtensions.SampleTile.Height;
            var halfTileWidth = tileWidth/2;
            var halfTileHeight = tileHeight/2;
            const int xSpace = 2;
            const int halfXSpace = 1;

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            try
            {
                using (var inputBitmap = new Bitmap(inputFile))
                {
                    var n = Math.Ceiling((double) (inputBitmap.Width - initTileX)/(tileWidth + xSpace));
                    var m = Math.Ceiling((double) (inputBitmap.Height - initTileY)/tileHeight)*2;
                    for (var i = 0; i < n; i++)
                    {
                        for (var j = 0; j < m; j++)
                        {
                            var evenRow = j%2;
                            var oddRow = 1 - evenRow;
                            using (var outputBitmap = inputBitmap.CreateTile(
                                initTileX + i*tileWidth - halfTileWidth*oddRow + i*xSpace + halfXSpace*evenRow,
                                initTileY + j*halfTileHeight - halfTileHeight))
                            {
                                if (outputBitmap.IsTransparent())
                                {
                                    Console.WriteLine($"Transparent tile at {j},{i}");
                                }
                                else
                                {
                                    try
                                    {
                                        var tilePath =
                                            $"{outputFolder.TrimEnd('/', '\\')}\\tile_{LeadingZero(j)}_{LeadingZero(i)}.bmp";
                                        Console.WriteLine(tilePath);
                                        outputBitmap.Save(tilePath, ImageFormat.Bmp);
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
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
            }
        }

        private static string LeadingZero(int i)
        {
            return i < 100
                ? i < 10
                    ? $"00{i}"
                    : $"0{i}"
                : $"{i}";
        }
    }
}