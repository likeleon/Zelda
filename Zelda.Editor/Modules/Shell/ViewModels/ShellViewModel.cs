using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.MainMenu;
using Zelda.Editor.Modules.Shell.Views;
using Zelda.Editor.Modules.StatusBar;

namespace Zelda.Editor.Modules.Shell.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel : Conductor<IDocument>.Collection.OneActive, IShell
    {
        public event EventHandler ActiveDocumentChanging;
        public event EventHandler ActiveDocumentChanged;

#pragma warning disable 649
        [ImportMany(typeof(IModule))]
        private IEnumerable<IModule> _modules;

        [Import]
        private IMenu _mainMenu;

        [Import]
        private IStatusBar _statusBar;
#pragma warning restore 649

        private IShellView _shellView;
        private bool _closing;

        public IMenu MainMenu
        {
            get { return _mainMenu; }
        }

        public IStatusBar StatusBar
        {
            get { return _statusBar; }
        }

        public ILayoutItem _activeLayoutItem;
        public ILayoutItem ActiveLayoutItem
        {
            get { return _activeLayoutItem; }
            set
            {
                if (ReferenceEquals(_activeLayoutItem, value))
                    return;

                _activeLayoutItem = value;

                if (value is IDocument)
                    ActivateItem(value as IDocument);

                NotifyOfPropertyChange(() => ActiveLayoutItem);
            }
        }

        public IDocument ActiveDocument
        {
            get { return ActiveItem; }
        }

        private readonly BindableCollection<ITool> _tools = new BindableCollection<ITool>();
        public IObservableCollection<ITool> Tools
        {
            get { return _tools; }
        }

        public IObservableCollection<IDocument> Documents
        {
            get { return Items; }
        }

        public virtual string StateFile
        {
            get { return @".\ApplicationState.bin"; }
        }

        public bool HasPersistedState
        {
            get { return File.Exists(StateFile); }
        }
        
        public ShellViewModel()
        {
            (this as IActivate).Activate();
        }

        protected override void OnViewLoaded(object view)
        {
            foreach (IModule module in _modules)
                foreach (var globalResourceDictionary in module.GlobalResourceDictionaries)
                    Application.Current.Resources.MergedDictionaries.Add(globalResourceDictionary);

            foreach (IModule module in _modules)
                module.PreInitialize();
            foreach (IModule module in _modules)
                module.Initialize();

            _shellView = view as IShellView;
            if (!HasPersistedState)
            {
                foreach (IDocument defaultDocument in _modules.SelectMany(x => x.DefaultDocuments))
                    OpenDocument(defaultDocument);
                foreach (Type defaultTool in _modules.SelectMany(x => x.DefaultTools))
                    ShowTool(IoC.GetInstance(defaultTool, null) as ITool);
            }

            foreach (IModule module in _modules)
                module.PostInitialize();

            base.OnViewLoaded(view);
        }

        public void ShowTool<T>() where T : ITool
        {
            ShowTool(IoC.Get<T>());
        }

        public void ShowTool(ITool tool)
        {
            if (Tools.Contains(tool))
                tool.IsVisible = true;
            else
                Tools.Add(tool);
            tool.IsSelected = true;
            ActiveLayoutItem = tool;
        }

        public void OpenDocument(IDocument doc)
        {
            ActivateItem(doc);
        }

        public void CloseDocument(IDocument doc)
        {
            DeactivateItem(doc, true);
        }

        public override void ActivateItem(IDocument doc)
        {
            if (_closing)
                return;

            RaiseActiveDocumentChanging();

            IDocument currentActiveDoc = ActiveDocument;

            base.ActivateItem(doc);

            if (!ReferenceEquals(doc, currentActiveDoc))
                RaiseActiveDocumentChanged();
        }

        private void RaiseActiveDocumentChanging()
        {
            EventHandler handler = ActiveDocumentChanging;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void RaiseActiveDocumentChanged()
        {
            EventHandler handler = ActiveDocumentChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected override void OnActivationProcessed(IDocument doc, bool success)
        {
            if (!ReferenceEquals(ActiveDocument, doc))
                ActiveLayoutItem = doc;

            base.OnActivationProcessed(doc, success);
        }

        public override void DeactivateItem(IDocument doc, bool close)
        {
            RaiseActiveDocumentChanging();

            base.DeactivateItem(doc, close);

            RaiseActiveDocumentChanged();
        }

        protected override void OnDeactivate(bool close)
        {
            _closing = true;

            base.OnDeactivate(close);
        }

        public void Close()
        {
            Application.Current.MainWindow.Close();
        }
    }
}
