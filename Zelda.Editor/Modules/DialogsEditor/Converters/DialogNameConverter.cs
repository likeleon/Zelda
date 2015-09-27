using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Zelda.Editor.Modules.DialogsEditor.Models;

namespace Zelda.Editor.Modules.DialogsEditor.Converters
{
    class DialogNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var node = value as Node;
            if (node == null)
                throw new ArgumentNullException("value");

            return node.Key.Split('.').Last();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
