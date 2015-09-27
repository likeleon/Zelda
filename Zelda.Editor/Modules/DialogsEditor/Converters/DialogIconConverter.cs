using System;
using System.Globalization;
using System.Windows.Data;
using Zelda.Editor.Core.Primitives;
using Zelda.Editor.Modules.DialogsEditor.Models;

namespace Zelda.Editor.Modules.DialogsEditor.Converters
{
    class DialogIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var node = value as Node;
            if (node == null)
                throw new ArgumentNullException("value");

            var model = (parameter as BindingProxy).Data as DialogsModel;
            if (model == null)
                throw new ArgumentNullException("parameter");

            return model.GetIcon(node);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
