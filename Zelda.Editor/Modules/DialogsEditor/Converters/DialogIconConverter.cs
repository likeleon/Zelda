using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.DialogsEditor.Models;

namespace Zelda.Editor.Modules.DialogsEditor.Converters
{
    class DialogIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dialog = value as Dialog;
            if (dialog == null)
                throw new ArgumentNullException("dialog");

            if (dialog.Type == NodeType.Container)
                return "/Resources/Icons/icon_folder_open.png".ToIconUri();

            if (dialog.Children.Any())
                return "/Resources/Icons/icon_dialogs.png".ToIconUri();
            else
                return "/Resources/Icons/icon_dialog.png".ToIconUri();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
