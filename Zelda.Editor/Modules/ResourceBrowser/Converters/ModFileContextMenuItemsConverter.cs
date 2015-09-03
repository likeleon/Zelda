using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Zelda.Editor.Core.Primitives;
using Zelda.Editor.Modules.ResourceBrowser.ViewModels;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser.Converters
{
    class ModFileContextMenuItemsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType is IEnumerable)
                throw new InvalidOperationException("targetType is not '{0}'".F(typeof(IEnumerable)));

            var modFile = value as IModFile;
            if (modFile == null)
                return Enumerable.Empty<IModFile>();

            var browser = (parameter as BindingProxy).Data as ResourceBrowserViewModel;
            if (browser == null)
                throw new ArgumentNullException("parameter", "parameter should be ResourceBrowserViewModel");

            var builder = new ModFileContextMenuBuilder(browser, modFile);
            return builder.Build();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
