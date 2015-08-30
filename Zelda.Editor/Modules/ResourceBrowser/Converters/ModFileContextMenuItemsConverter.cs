using Caliburn.Micro;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Zelda.Editor.Core.Mods;
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

            var mod = IoC.Get<IModService>().Mod;
            var builder = new ModFileContextMenuBuilder(mod, modFile.Path);
            return builder.Build();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
