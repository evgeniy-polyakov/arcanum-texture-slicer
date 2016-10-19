using System.Windows.Input;

namespace ArcanumTextureSlicer.Gui.Commands
{
    public static class Commands
    {
        public static readonly RoutedCommand MoveGridUp = new RoutedCommand("MoveGridUp", typeof(MainWindow),
            new InputGestureCollection { new KeyGesture(Key.Up, ModifierKeys.Control) });

        public static readonly RoutedCommand MoveGridDown = new RoutedCommand("MoveGridDown", typeof(MainWindow),
            new InputGestureCollection { new KeyGesture(Key.Down, ModifierKeys.Control) });

        public static readonly RoutedCommand MoveGridLeft = new RoutedCommand("MoveGridLeft", typeof(MainWindow),
            new InputGestureCollection {new KeyGesture(Key.Left, ModifierKeys.Control)});

        public static readonly RoutedCommand MoveGridRight = new RoutedCommand("MoveGridRight", typeof(MainWindow),
            new InputGestureCollection {new KeyGesture(Key.Right, ModifierKeys.Control)});
    }
}