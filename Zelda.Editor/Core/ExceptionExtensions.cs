using System;
using System.Windows;

namespace Zelda.Editor.Core
{
    public static class ExceptionExtensions
    {
        public static void ShowDialog(this Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
