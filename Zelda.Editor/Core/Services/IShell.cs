using Caliburn.Micro;
using System;
using Zelda.Editor.Modules.MainMenu;
using Zelda.Editor.Modules.StatusBar;
using Zelda.Editor.Modules.ToolBars;

namespace Zelda.Editor.Core.Services
{
    interface IShell : IGuardClose, IDeactivate
    {
        event EventHandler ActiveDocumentChanging;
        event EventHandler ActiveDocumentChanged;

        IMenu MainMenu { get; }
        IToolBars ToolBars { get; }
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
