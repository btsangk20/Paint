using System.Windows;
using System.Windows.Controls;

namespace Paint.Commands
{
    public class AddCommand : Command
    {
        public AddCommand(Canvas canvas, UIElement element) : base(canvas, element)
        {
        }

        public override void Undo()
        {
            Canvas.Children.Remove(Element);
        }

        public override void Redo()
        {
            Canvas.Children.Add(Element);
        }
    }
}
