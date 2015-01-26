using System.Windows;

namespace Zelda.Editor.Core.Commands
{
    public interface ICommandKeyGestureService
    {
        void BindKeyGestures(UIElement uiElement);
    }
}
