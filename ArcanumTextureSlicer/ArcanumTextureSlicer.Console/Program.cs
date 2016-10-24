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

                    foreach (var tile in inputBitmap.ToTiles(initTileX, initTileY))
                    {
                        if (tile.Row == 0 && tile.Column == 0 &&
                            initTileCenter.X == initTileX &&
                            initTileCenter.Y == initTileY)
                        {
                            // Do not export start tile
                            continue;
                        }
                        using (var outputBitmap = inputBitmap.CreateTile(tile.X, tile.Y))
                        {
                            if (outputBitmap.IsTransparent())
                            {
                                System.Console.WriteLine($"Transparent tile at {tile.Row},{tile.Column}");
                            }
                            else
                            {
                                try
                                {
                                    var tilePath =
                                        $"{outputFolder.TrimEnd('/', '\\')}\\tile_{tile.Row.ToString("D3")}_{tile.Column.ToString("D3")}.bmp";
                                    System.Console.WriteLine(tilePath);
                                    outputBitmap.Save(tilePath, ImageFormat.Bmp);
                                }
                                catch (Exception e)
                                {
                                    System.Console.WriteLine(e);
                                }
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                System.Console.WriteLine(e);
            }
        }
    }
}