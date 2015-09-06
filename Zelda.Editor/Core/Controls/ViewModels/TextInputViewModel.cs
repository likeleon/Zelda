using Caliburn.Micro;
using System.Dynamic;
using System.Windows;

namespace Zelda.Editor.Core.Controls.ViewModels
{
    class TextInputViewModel : WindowBase
    {
        public string Title { get; set; }
        public string Label { get; set; }
        public string Text { get; set; }

        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public TextInputViewModel()
        {
            OkCommand = new RelayCommand(o => TryClose(true));
            CancelCommand = new RelayCommand(o => TryClose(false));
        }

        public static string GetText(string title, string label, string text)
        {
            var dialog = new TextInputViewModel() { Title = title, Label = label, Text = text };
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (IoC.Get<IWindowManager>().ShowDialog(dialog, null, settings) != true)
                return null;

            return dialog.Text;
        }
    }
}
