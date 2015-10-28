using Caliburn.Micro;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

namespace Zelda.Editor.Core.Services
{
    static class ExtensionMethods
    {
        public static string GetExecutingAssemblyName()
        {
            return Assembly.GetExecutingAssembly().GetAssemblyName();
        }

        public static string GetPropertyName<TProperty>(Expression<Func<TProperty>> property)
        {
            return property.GetMemberInfo().Name;
        }

        public static string GetPropertyName<TTarget, TProperty>(Expression<Func<TTarget, TProperty>> property)
        {
            return property.GetMemberInfo().Name;
        }

        public static Uri ToIconUri(this string iconFile)
        {
            var path = "/Resources/Icons/" + iconFile;
            return new Uri(path, UriKind.Relative);
        }

        public static void ShowWarningDialog(this string text)
        {
            MessageBox.Show(text, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void ShowErrorDialog(this string text)
        {
            MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool AskYesNo(this string question, string dialogTitle)
        {
            var answer = MessageBox.Show(question, dialogTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return answer == MessageBoxResult.Yes;
        }

        public static bool? AskYesNoCancel(this string question, string dialogTitle)
        {
            var answer = MessageBox.Show(question, dialogTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Yes)
                return true;
            else if (answer == MessageBoxResult.No)
                return false;
            else
                return null;
        }
    }
}
