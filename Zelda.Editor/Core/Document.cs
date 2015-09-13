using System.Windows.Input;

namespace Zelda.Editor.Core
{
    class Document : LayoutItemBase, IDocument
    {
        ICommand _closeCommand;

        public override ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(_ => TryClose(null), _ => true)); }
        }
    }
}
