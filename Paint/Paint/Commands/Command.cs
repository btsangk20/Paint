using System.Windows;
using System.Windows.Controls;

namespace Paint.Commands
{
    public abstract class Command
    {
        protected Canvas Canvas;
        protected UIElement Element;

        protected Command(Canvas canvas, UIElement element)
        {
            this.Canvas = canvas;
            this.Element = element;
        }

        public abstract void Undo();
        public abstract void Redo();
    }
}
