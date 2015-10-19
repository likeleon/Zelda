using Caliburn.Micro;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Threading;
using Zelda.Editor.Core.ToolBars;
using Zelda.Editor.Modules.Shell.Commands;
using Zelda.Editor.Modules.ToolBars;
using Zelda.Editor.Modules.ToolBars.Models;
using Zelda.Editor.Modules.UndoRedo;
using Zelda.Editor.Modules.UndoRedo.Commands;
using Zelda.Editor.Modules.UndoRedo.Services;

namespace Zelda.Editor.Core
{
    class Document : LayoutItemBase, IDocument,
        ICommandHandler<UndoCommandDefinition>,
        ICommandHandler<RedoCommandDefinition>,
        ICommandHandler<SaveFileCommandDefinition>
    {
        ICommand _closeCommand;
        IUndoRedoManager _undoRedoManager;
        ToolBarDefinition _toolBarDefinition;
        IToolBar _toolBar;

        public override ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(_ => TryClose(null), _ => true)); }
        }

        public ToolBarDefinition ToolBarDefinition
        {
            get { return _toolBarDefinition; }
            protected set
            {
                _toolBarDefinition = value;
                NotifyOfPropertyChange(() => ToolBar);
                NotifyOfPropertyChange();
            }
        }

        public IToolBar ToolBar
        {
            get
            {
                if (_toolBar != null)
                    return _toolBar;

                if (ToolBarDefinition == null)
                    return null;

                var toolBarBuilder = IoC.Get<IToolBarBuilder>();
                _toolBar = new ToolBarModel();
                toolBarBuilder.BuildToolBar(_toolBarDefinition, _toolBar);
                return _toolBar;
            }
        }

        public IUndoRedoManager UndoRedoManager
        {
            get { return _undoRedoManager ?? (_undoRedoManager = new UndoRedoManager()); }
        }

        void ICommandHandler<UndoCommandDefinition>.Update(Command command)
        {
            command.Enabled = UndoRedoManager.UndoStack.Any();
        }

        Task ICommandHandler<UndoCommandDefinition>.Run(Command command)
        {
            UndoRedoManager.Undo(1);
            return TaskUtility.Completed;
        }

        void ICommandHandler<RedoCommandDefinition>.Update(Command command)
        {
            command.Enabled = UndoRedoManager.RedoStack.Any();
        }

        Task ICommandHandler<RedoCommandDefinition>.Run(Command command)
        {
            UndoRedoManager.Redo(1);
            return TaskUtility.Completed;
        }

        void ICommandHandler<SaveFileCommandDefinition>.Update(Command command)
        {
            command.Enabled = this is ISavableDocument;
        }

        async Task ICommandHandler<SaveFileCommandDefinition>.Run(Command command)
        {
            var savableDocument = this as ISavableDocument;
            if (savableDocument == null)
                return;

            await savableDocument.Save();
        }
    }
}
