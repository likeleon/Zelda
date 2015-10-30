using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Zelda.Editor.Modules.StringsEditor.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.StringsEditor.Converters
{
    class KeyToTranslatedStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            if (values.Length != 2)
                throw new ArgumentException("Expected 2 values, actual {0}".F(values.Length), "values");

            var key = values[0] as string;
            var model = values[1] as StringsModel;
            if (key == null || model == null)
                return DependencyProperty.UnsetValue;

            return model.GetTranslatedString(key);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
