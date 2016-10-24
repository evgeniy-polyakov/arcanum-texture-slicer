using System.Windows.Input;

namespace ArcanumTextureSlicer.Gui.Commands
{
    public class ExportCommand : RoutedCommand
    {
        public ExportCommand() : base("Export", typeof(MainWindow), new InputGestureCollection
        {
            new KeyGesture(Key.E, ModifierKeys.Control)
        })
        {
        }
    }
}