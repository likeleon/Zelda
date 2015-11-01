using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Zelda.Editor.Modules.DialogsEditor.Models;

namespace Zelda.Editor.Modules.StringsEditor.Converters
{
    class SortStringNodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nodes = value as IDictionary<string, Node>;
            if (nodes == null)
                return DependencyProperty.UnsetValue;

            var listCollectionView = CollectionViewSource.GetDefaultView(nodes);
            listCollectionView.SortDescriptions.Add(new SortDescription("Key", ListSortDirection.Ascending));
            return listCollectionView;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
