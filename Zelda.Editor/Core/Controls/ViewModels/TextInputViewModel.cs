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
    }
}
