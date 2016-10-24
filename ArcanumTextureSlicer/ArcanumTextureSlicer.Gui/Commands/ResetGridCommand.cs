using System.Windows.Input;

namespace ArcanumTextureSlicer.Gui.Commands
{
    public class ResetGridCommand : RoutedCommand
    {
        public ResetGridCommand() : base("ResetGrid", typeof(MainWindow), new InputGestureCollection
        {
            new KeyGesture(Key.D0, ModifierKeys.Control),
            new KeyGesture(Key.NumPad0, ModifierKeys.Control)
        })
        {
        }
    }
}