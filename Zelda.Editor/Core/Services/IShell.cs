using Caliburn.Micro;
using System;
using Zelda.Editor.Modules.MainMenu;
using Zelda.Editor.Modules.StatusBar;

namespace Zelda.Editor.Core.Services
{
    public interface IShell
    {
        event EventHandler ActiveDocumentChanging;
        event EventHandler ActiveDocumentChanged;

        IMenu MainMenu { get; }
        IStatusBar StatusBar { get; }

        ILayoutItem ActiveLayoutItem { get; set; }
        IDocument ActiveDocument { get; }

        IObservableCollection<IDocument> Documents { get; }
        IObservableCollection<ITool> Tools { get; }

        void ShowTool<TTool>() where TTool : ITool;
        void ShowTool(ITool tool);

        void OpenDocument(IDocument document);
        void CloseDocument(IDocument document);

        void Close();
    }
}
