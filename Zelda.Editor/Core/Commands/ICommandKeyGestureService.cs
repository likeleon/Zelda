using System.Windows;
using System.Windows.Input;

namespace Zelda.Editor.Core.Commands
{
    public interface ICommandKeyGestureService
    {
        void BindKeyGestures(UIElement uiElement);
    }
}
