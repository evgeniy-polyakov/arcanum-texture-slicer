using System.Windows.Input;

namespace ArcanumTextureSlicer.Gui.Commands
{
    public class ZoomCommand : RoutedCommand
    {
        public ZoomCommand(int zoom) : base($"Zoom_x{zoom}", typeof(MainWindow), GetInputGestures(zoom))
        {
            Zoom = zoom;
        }

        public int Zoom { get; private set; }

        private static InputGestureCollection GetInputGestures(int zoom)
        {
            Key keyDigit;
            Key keyNumpad;
            switch (zoom)
            {
                case 4:
                    keyDigit = Key.D4;
                    keyNumpad = Key.NumPad4;
                    break;
                case 3:
                    keyDigit = Key.D3;
                    keyNumpad = Key.NumPad3;
                    break;
                case 2:
                    keyDigit = Key.D2;
                    keyNumpad = Key.NumPad2;
                    break;
                default:
                    keyDigit = Key.D1;
                    keyNumpad = Key.NumPad1;
                    break;
            }
            return new InputGestureCollection
            {
                new KeyGesture(keyDigit, ModifierKeys.Control),
                new KeyGesture(keyNumpad, ModifierKeys.Control)
            };
        }
    }
}