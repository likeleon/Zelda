using System;
using System.IO;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Editor.Modules.UndoRedo;
using Zelda.Game;

namespace Zelda.Editor.Modules.Mods.ViewModels
{
    abstract class EditorDocument : Document, ISavableDocument
    {
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public Mods.Models.ModResources Resources { get { return Mod.Resources; } }
        public bool HasUnsavedChanges { get { return UndoRedoManager.UndoStack.Count > 0; } }

        protected IMod Mod { get; private set; }
        protected string CloseConfirmMessage { get; set; }

        public override void CanClose(Action<bool> callback)
        {
            callback(ConfirmClose());
        }

        protected EditorDocument(IMod mod, string filePath)
        {
            Mod = mod;
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            CloseConfirmMessage = "File '{0}' has been modified. Save changes?".F(FileName);

            UndoRedoManager.UndoStack.CollectionChanged += (_, e) => UpdateDisplayName();
        }

        void UpdateDisplayName()
        {
            DisplayName = (HasUnsavedChanges) ? FileName + "*" : FileName;
        }

        public bool ConfirmClose()
        {
            if (!HasUnsavedChanges)
                return true;

            var answer = CloseConfirmMessage.AskYesNoCancel("Save changes");
            if (!answer.HasValue)
                return false;   // Don't close

            if (answer.Value)   // Save and close
            {
                try
                {
                    OnSave();
                    UndoRedoManager.UndoStack.Clear();
                    return true;
                }
                catch (Exception ex)
                {
                    ex.ShowDialog();
                    return false;
                }
            }
            else
            {
                return true;    // Close without saving
            }
        }

        protected abstract Task OnSave();

        protected void TryAction(IUndoableAction action)
        {
            try
            {
                action.Execute();
                UndoRedoManager.ExecuteAction(new UndoActionSkipFirst(action));
            }
            catch (Exception ex)
            {
                ex.ShowDialog();
            }
        }

        public Task Save()
        {
            return OnSave();
        }

        class UndoActionSkipFirst : IUndoableAction
        {
            readonly IUndoableAction _wrappedCommand;
            bool _firstTime = true;

            public string Name { get { return _wrappedCommand.Name; } }

            public UndoActionSkipFirst(IUndoableAction wrappedCommand)
            {
                _wrappedCommand = wrappedCommand;
            }

            public void Execute()
            {
                if (_firstTime)
                {
                    _firstTime = false;
                    return;
                }

                try
                {
                    _wrappedCommand.Execute();
                }
                catch (Exception ex)
                {
                    ex.ShowDialog();
                }
            }

            public void Undo()
            {
                try
                {
                    _wrappedCommand.Undo();
                }
                catch (Exception ex)
                {
                    ex.ShowDialog();
                }
            }
        }
    }
}
