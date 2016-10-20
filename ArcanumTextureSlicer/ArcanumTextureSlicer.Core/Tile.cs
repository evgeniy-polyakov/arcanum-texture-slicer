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
            outline[0] = new Point(38, 0);
            outline[1] = new Point(39, 0);
            for (var i = 1; i <= 38; i++)
            {
                var j = 2 + (i - 1)*2;
                outline[j] = new Point(HalfWidth - Rows[i]/2, i);
                outline[j + 1] = new Point(HalfWidth + Rows[i]/2 - 1, i);
            }
            outline[outline.Length - 2] = new Point(38, 39);
            outline[outline.Length - 1] = new Point(39, 39);
            return outline;
        }

        public static bool HitTest(int x, int y)
        {
            return y >= 0 && y < Rows.Length
                   && x >= (Width - Rows[y])/2
                   && x < (Width + Rows[y])/2;
        }

        public int Row;
        public int Column;
        public int X;
        public int Y;
    }
}