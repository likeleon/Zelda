using System.Windows;
using System.Windows.Controls;
using Zelda.Editor.Core.Controls;
using Zelda.Editor.Modules.ToolBars.Models;

namespace Zelda.Editor.Modules.ToolBars.Controls
{
    class ToolBarBase : ToolBar
    {
        object _currentItem;

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            _currentItem = item;
            return base.IsItemItsOwnContainerOverride(item);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            if (_currentItem is ToolBarItemSeparator)
                return new Separator();

            if (_currentItem is CommandToolBarItem)
                return CreateButton<CustomToggleButton>(ToggleButtonStyleKey, "ToolBarButton");

            return base.GetContainerForItemOverride();
        }

        static T CreateButton<T>(object baseStyleKey, string styleKey)
            where T : FrameworkElement, new()
        {
            var result = new T();
            result.SetResourceReference(DynamicStyle.BaseStyleProperty, baseStyleKey);
            result.SetResourceReference(DynamicStyle.DerivedStyleProperty, styleKey);
            return result;
        }
    }
}