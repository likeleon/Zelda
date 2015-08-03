using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Zelda.Editor.Modules.ResourceBrowser.Converters
{
    class LevelConverter : DependencyObject, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue)
                return 0.0;

            var level = (int)values[0];
            var indent = (double)values[1];
            return indent * level;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
