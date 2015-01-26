using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Core
{
    public interface ITool : ILayoutItem
    {
        PaneLocation PreferredLocation { get; }
        double PreferredWidth { get; }
        double PreferredHeight { get; }

        bool IsVisible { get; set; }
    }
}
