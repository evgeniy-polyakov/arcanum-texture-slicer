using System;
using System.Windows.Input;

namespace ArcanumTextureSlicer.Gui.Commands
{
    public class MoveGridCommand : RoutedCommand
    {
        public MoveGridCommand(int x, int y) : base($"MoveGrid_{x}_{y}", typeof(MainWindow), GetInputGestures(x, y))
        {
            X = x;
            Y = y;
        }

        public int X { get; private set; }
        public int Y { get; private set; }

        private static InputGestureCollection GetInputGestures(int x, int y)
        {
            var key = x < 0
                ? Key.Left
                : x > 0
                    ? Key.Right
                    : y < 0
                        ? Key.Up
                        : y > 0
                            ? Key.Down
                            : Key.None;
            if (key == Key.None)
            {
                return new InputGestureCollection();
            }
            var shift = Math.Abs(x) > 1 || Math.Abs(y) > 1
                ? ModifierKeys.Shift
                : ModifierKeys.None;
            return new InputGestureCollection {new KeyGesture(key, ModifierKeys.Control | shift)};
        }
    }
}