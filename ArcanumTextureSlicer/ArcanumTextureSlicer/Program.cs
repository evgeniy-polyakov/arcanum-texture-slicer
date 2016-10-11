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

            const int tileWidth = BitmapExtensions.TileWidth;
            const int tileHeight = BitmapExtensions.TileHeight;
            const int halfTileWidth = BitmapExtensions.HalfTileWidth;
            const int halfTileHeight = BitmapExtensions.HalfTileHeight;
            const int xSpace = BitmapExtensions.TileXSpace;
            const int halfXSpace = BitmapExtensions.HalfTileXSpace;

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            try
            {
                using (var inputBitmap = new Bitmap(inputFile))
                {
                    var initTileCenter = inputBitmap.GetStartTileCenter();
                    var initTileX = args.Length > 2 ? int.Parse(args[2]) : initTileCenter.X;
                    var initTileY = args.Length > 3 ? int.Parse(args[3]) : initTileCenter.Y;

                    var n = Math.Ceiling((double) (inputBitmap.Width - initTileX)/(tileWidth + xSpace));
                    var m = Math.Ceiling((double) (inputBitmap.Height - initTileY)/tileHeight)*2;
                    for (var i = 0; i < n; i++)
                    {
                        for (var j = 0; j < m; j++)
                        {
                            if (i == 0 && j == 0 && !initTileCenter.IsEmpty)
                            {
                                // Do not export start tile
                                continue;
                            }
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