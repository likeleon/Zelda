using System;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Core
{
    public static class ExceptionExtensions
    {
        public static void ShowDialog(this Exception ex)
        {
            ex.Message.ShowErrorDialog();
        }
    }
}
