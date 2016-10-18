using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using ArcanumTextureSlicer.Core;

namespace ArcanumTextureSlicer.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                System.Console.WriteLine($"Not enough arguments.");
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

            if (Directory.Exists(outputFolder))
            {
                Directory.GetFiles(outputFolder, "tile_???_???.bmp").ToList().ForEach(File.Delete);
            }
            else
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

                    inputBitmap.IterateTiles(initTileX, initTileY, p =>
                    {
                        if (p.Row == 0 && p.Column == 0 &&
                            initTileCenter.X == initTileX && initTileCenter.Y == initTileY)
                        {
                            // Do not export start tile
                            return;
                        }
                        using (var outputBitmap = p.Bitmap.CreateTile(p.X, p.Y))
                        {
                            if (outputBitmap.IsTransparent())
                            {
                                System.Console.WriteLine($"Transparent tile at {p.Row},{p.Column}");
                            }
                            else
                            {
                                try
                                {
                                    var tilePath =
                                        $"{outputFolder.TrimEnd('/', '\\')}\\tile_{LeadingZero(p.Row)}_{LeadingZero(p.Column)}.bmp";
                                    System.Console.WriteLine(tilePath);
                                    outputBitmap.Save(tilePath, ImageFormat.Bmp);
                                }
                                catch (Exception e)
                                {
                                    System.Console.WriteLine(e);
                                }
                            }
                        }
                    });
                }
            }
            catch (FileNotFoundException e)
            {
                System.Console.WriteLine(e);
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