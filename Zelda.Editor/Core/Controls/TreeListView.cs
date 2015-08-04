using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Zelda.Game;

namespace Zelda.Editor.Core.Controls
{
    class TreeListViewItem : TreeViewItem
    {
        readonly Lazy<int> _level;

        public int Level { get { return _level.Value; } }

        public TreeListViewItem()
        {
            _level = Exts.Lazy(() =>
            {
                var parent = ItemsControl.ItemsControlFromItemContainer(this) as TreeListViewItem;
                return (parent != null) ? parent.Level + 1 : 0;
            });
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }
    }

    class TreeListView : TreeView
    {
        readonly Lazy<GridViewColumnCollection> _columns = Exts.Lazy(() => new GridViewColumnCollection());

        public GridViewColumnCollection Columns { get { return _columns.Value; } }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }
    }

    class LevelToIndentConverter : IValueConverter
    {
        const double IndentSize = 17.0;

        public object Convert(object o, Type type, object parameter, CultureInfo culture)
        {
            return new Thickness((int)o * IndentSize, 0, 0, 0);
        }

        public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
