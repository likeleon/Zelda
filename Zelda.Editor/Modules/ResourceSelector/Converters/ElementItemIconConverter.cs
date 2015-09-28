using System;
using System.Globalization;
using System.Windows.Data;
using Zelda.Editor.Core.Primitives;
using Zelda.Editor.Modules.ResourceSelector.Models;

namespace Zelda.Editor.Modules.ResourceSelector.Converters
{
    class ElementItemIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var elementItem = value as ElementItem;
            if (elementItem == null)
                throw new ArgumentNullException("value");

            var model = (parameter as BindingProxy).Data as ResourceModel;
            if (model == null)
                throw new ArgumentNullException("parameter");

            return model.GetIcon(elementItem);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
