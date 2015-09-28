using System.Windows;
using System.Windows.Controls;
using Zelda.Editor.Modules.ResourceSelector.Models;

namespace Zelda.Editor.Modules.ResourceSelector
{
    class ItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DirectoryItemTemplate { get; set; }
        public DataTemplate ElementItemTemplate { get; set; }
        public DataTemplate SpecialValueItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DirectoryItem)
                return DirectoryItemTemplate;
            else if (item is ElementItem)
                return ElementItemTemplate;
            else if (item is SpecialValueItem)
                return SpecialValueItemTemplate;
            else
                return base.SelectTemplate(item, container);
        }
    }
}
