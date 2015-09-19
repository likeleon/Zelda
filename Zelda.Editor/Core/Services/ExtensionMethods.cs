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

        public static Uri ToIconUri(this string path)
        {
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
    }
}
