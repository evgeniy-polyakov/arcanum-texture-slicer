using System;
using System.Collections.Generic;
using System.Drawing;

namespace ArcanumTextureSlicer.Core
{
    public struct Tile
    {
        public const int Width = 78;
        public const int HalfWidth = 39;
        public const int Height = 40;
        public const int HalfHeight = 20;
        public const int XSpace = 2;
        public const int HalfXSpace = 1;

        private static int[] _rows;
        private static Point[] _outline;

        public static int[] Rows => _rows ?? (_rows = GetRows());
        public static Point[] Outline => _outline ?? (_outline = GetOutline());

        private static int[] GetRows()
        {
            var rows = new int[40];
            for (var i = 0; i <= 19; i++)
            {
                rows[i] = 2 + i*4;
            }
            for (var i = 19; i >= 0; i--)
            {
                rows[39 - i] = 2 + i*4;
            }
            return rows;
        }

        private static Point[] GetOutline()
        {
            var outline = new Point[38*2 + 4];
            outline[0] = new Point(-1, -20);
            outline[1] = new Point(0, -20);
            for (var i = 1; i <= 38; i++)
            {
                var j = 2 + (i - 1)*2;
                outline[j] = new Point(-Rows[i]/2, i - 20);
                outline[j + 1] = new Point(Rows[i]/2 - 1, i - 20);
            }
            outline[outline.Length - 2] = new Point(-1, 19);
            outline[outline.Length - 1] = new Point(0, 19);
            return outline;
        }

        public static bool HitTest(int x, int y)
        {
            return y >= -HalfHeight && y < HalfHeight
                   && x >= -Rows[y + HalfHeight]/2
                   && x < Rows[y + HalfHeight]/2;
        }

        public static IEnumerable<Tile> Split(int width, int height)
        {
            var rows = Math.Ceiling((decimal) height/Height)*2 + 1;
            var columns = Math.Ceiling((decimal) width/(Width + XSpace)) + 1;
            for (var row = 0; row < rows; row++)
            {
                var evenRow = row%2;
                for (var column = 0; column < columns; column++)
                {
                    var lastColumn = column == columns - 1;
                    if (lastColumn && evenRow > 0)
                    {
                        continue;
                    }
                    yield return new Tile
                    {
                        Row = row,
                        Column = column,
                        X = (Width + XSpace)*column + (HalfWidth + HalfXSpace)*evenRow,
                        Y = row*HalfHeight
                    };
                }
            }
        }

        public int Row;
        public int Column;
        public int X;
        public int Y;
    }
}