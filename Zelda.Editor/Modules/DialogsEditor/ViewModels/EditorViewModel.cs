using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Controls.ViewModels;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;
using Zelda.Editor.Modules.DialogsEditor.Models;
using Zelda.Editor.Modules.ResourceSelector.Models;
using Zelda.Editor.Modules.ResourceSelector.ViewModels;
using Zelda.Editor.Modules.UndoRedo;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
{
    class EditorViewModel : PersistedDocument
    {
        readonly IMod _mod;
        string _languageId;
        Node _selectedDialogNode;
        bool _isDialogPropertiesEnabled;
        string _dialogId;
        string _dialogText;
        string _translationText;
        DialogPropertiesTable.Item _selectedPropertyItem;

        public string LanguageId
        {
            get { return _languageId; }
            private set
            {
                if (this.SetProperty(ref _languageId, value))
                    NotifyOfPropertyChange(() => Description);
            }
        }

        public DialogsModel DialogsModel { get; private set; }
        public DialogPropertiesTable PropertiesTable { get; private set; }

        public Node SelectedDialogNode
        {
            get { return _selectedDialogNode; }
            set
            {
                if (this.SetProperty(ref _selectedDialogNode, value))
                    UpdateSelection();
            }
        }

        public string Description
        {
            get
            {
                if (LanguageId.IsNullOrEmpty())
                    return "";

                return _mod.Resources.GetDescription(ResourceType.Language, LanguageId);
            }
            set { SetDescription(value); }
        }

        public override Uri IconSource { get { return "icon_resource_language.png".ToIconUri(); } }

        public SelectorViewModel TranslationSelector { get; private set; }

        string SelectedDialogId { get { return SelectedDialogNode != null ? SelectedDialogNode.Key : string.Empty; } }

        public bool IsDialogPropertiesEnabled
        {
            get { return _isDialogPropertiesEnabled; }
            set { this.SetProperty(ref _isDialogPropertiesEnabled, value); }
        }

        public string DialogId
        {
            get { return _dialogId; }
            set { this.SetProperty(ref _dialogId, value); }
        }

        public string DialogText
        {
            get { return _dialogText; }
            set { this.SetProperty(ref _dialogText, value); }
        }

        public string TranslationText
        {
            get { return _translationText; }
            set { this.SetProperty(ref _translationText, value); }
        }

        public DialogPropertiesTable.Item SelectedPropertyItem
        {
            get { return _selectedPropertyItem; }
            set
            {
                if (this.SetProperty(ref _selectedPropertyItem, value))
                    UpdatePropertiesButtons();
            }
        }

        public string SelectedPropertyKey
        {
            get { return _selectedPropertyItem != null ? _selectedPropertyItem.Key : string.Empty; }
        }

        public RelayCommand RefreshTranslationCommand { get; private set; }
        public RelayCommand CreatePropertyCommand { get; private set; }
        public RelayCommand SetPropertyKeyCommand { get; private set; }
        public RelayCommand DeletePropertyCommand { get; private set; }

        Core.Mods.ModResources Resources { get { return _mod.Resources; } }

        public EditorViewModel(IMod mod)
        {
            _mod = mod;
            Resources.ElementDescriptionChanged += (_, e) => NotifyOfPropertyChange(() => Description);

            RefreshTranslationCommand = new RelayCommand(_ => RefreshTranslation());
            CreatePropertyCommand = new RelayCommand(_ => CreateProperty());
            SetPropertyKeyCommand = new RelayCommand(_ => ChangeDialogPropertyKey(),
                                                     _ => DialogsModel != null && SelectedDialogPropertyExists());
            DeletePropertyCommand = new RelayCommand(_ => DeleteDialogProperty(),
                                                     _ => DialogsModel != null && SelectedDialogPropertyExists());
        }

        protected override Task DoLoad(string filePath)
        {
            var languageId = "";
            if (!_mod.IsDialogsFile(filePath, ref languageId))
                throw new InvalidOperationException("Path is not dialogs file: {0}".F(filePath));

            DisplayName = "Dialogs {0}".F(languageId);
            LanguageId = languageId;

            DialogsModel = new DialogsModel(_mod, languageId);
            NotifyOfPropertyChange(() => DialogsModel);

            PropertiesTable = new DialogPropertiesTable(DialogsModel);
            NotifyOfPropertyChange(() => PropertiesTable);

            TranslationSelector = new SelectorViewModel(_mod, ResourceType.Language);
            TranslationSelector.RemoveId(languageId);
            TranslationSelector.AddSpecialValue("", "<No language>", 0);
            TranslationSelector.SetSelectedId("");
            TranslationSelector.SelectedItemChanged += TranslationSelector_SelectedItemChanged;
            NotifyOfPropertyChange(() => TranslationSelector);

            return TaskUtility.Completed;
        }

        void TranslationSelector_SelectedItemChanged(object sender, Item e)
        {
            var newLanguage = e as ElementItem;
            if (newLanguage == null)
            {
                DialogsModel.ClearTranslation();
            }
            else
            {
                try
                {
                    DialogsModel.SetTranslationId(newLanguage.Id);
                }
                catch (Exception ex)
                {
                    ex.ShowDialog();
                }
            }
            UpdateTranslationText();
        }

        void SetDescription(string description)
        {
            if (description == Resources.GetDescription(ResourceType.Language, _languageId))
                return;

            if (description.Length <= 0)
            {
                "Invalid description".ShowErrorDialog();
                return;
            }

            try
            {
                Resources.SetDescription(ResourceType.Language, _languageId, description);
                Resources.Save();
            }
            catch (Exception e)
            {
                e.ShowDialog();
            }
        }

        void UpdateSelection()
        {
            UpdateDialogView();
        }

        void UpdateDialogView()
        {
            var exists = DialogsModel.DialogExists(SelectedDialogId);
            var hasTranslation = DialogsModel.TranslatedDialogExists(SelectedDialogId);
            IsDialogPropertiesEnabled = exists || hasTranslation;

            UpdateDialogId();
            UpdateDialogText();
            UpdateTranslationText();
            UpdatePropertiesButtons();

            PropertiesTable.DialogId = SelectedDialogId;
        }

        void UpdateDialogId()
        {
            if (DialogsModel.DialogExists(SelectedDialogId) || DialogsModel.TranslatedDialogExists(SelectedDialogId))
                DialogId = SelectedDialogId;
            else
                DialogId = null;
        }

        void UpdateDialogText()
        {
            if (DialogsModel.DialogExists(SelectedDialogId))
                DialogText = DialogsModel.GetDialogText(SelectedDialogId);
            else
                DialogText = null;
        }

        void UpdateTranslationText()
        {
            if (DialogsModel.TranslatedDialogExists(SelectedDialogId))
                TranslationText = DialogsModel.GetTranslatedDialogText(SelectedDialogId);
            else
                TranslationText = null;
        }

        void UpdatePropertiesButtons()
        {
            SetPropertyKeyCommand.RaiseCanExecuteChanged();
            DeletePropertyCommand.RaiseCanExecuteChanged();
        }

        bool SelectedDialogPropertyExists()
        {
            return DialogsModel.DialogPropertyExists(SelectedDialogId, SelectedPropertyKey);
        }

        void RefreshTranslation()
        {
            if (DialogsModel.TranslationId == null)
                return;

            DialogsModel.ReloadTranslation();
            UpdateTranslationText();
        }

        void CreateProperty()
        {
            var key = TextInputViewModel.GetText("New dialog property", "New property key:", SelectedPropertyKey);
            if (key == null)
                return;

            var id = SelectedDialogId;
            if (DialogsModel.DialogPropertyExists(id, key))
            {
                "The property '{0}' already exists in the dialog '{1}'".F(key, id).ShowErrorDialog();
                return;
            }

            if (!DialogsModel.DialogExists(id))
            {
                var properties = new Dictionary<string, string>() { { key, "" } };
                TryAction(new CreateDialogAction(this, id, "", properties));
            }
            else
            {
                TryAction(new CreateDialogPropertyAction(this, id, key, ""));
            }
        }

        void ChangeDialogPropertyKey()
        {
        }

        void DeleteDialogProperty()
        {
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

        void TryAction(IUndoableAction action)
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

        void SetSelectedDialog(string id)
        {
            SelectedDialogNode = DialogsModel.FindNode(id);
        }

        void SetSelectedProperty(string key)
        {
            SelectedPropertyItem = PropertiesTable.Items.FirstOrDefault(i => i.Key == key);
        }

        abstract class DialogsEditorAction : IUndoableAction
        {
            public EditorViewModel Editor { get; private set; }
            public DialogsModel Model { get; private set; }

            public string Name { get; private set; }

            public void Execute()
            {
                OnExecute();
            }

            public void Undo()
            {
                OnUndo();
            }

            public DialogsEditorAction(EditorViewModel editor, string name)
            {
                Editor = editor;
                Name = name;
                Model = editor.DialogsModel;
            }

            protected abstract void OnExecute();
            protected abstract void OnUndo();
        }

        class CreateDialogAction : DialogsEditorAction
        {
            readonly string _id;
            readonly string _text;
            Dictionary<string, string> _properties;

            public CreateDialogAction(EditorViewModel editor, string id, string text, Dictionary<string, string> properties)
                : base(editor, "Create dialog")
            {
                _id = id;
                _text = text;
                _properties = properties;
            }

            protected override void OnExecute()
            {
                Model.CreateDialog(_id, _text, _properties);
                Editor.SetSelectedDialog(_id);
            }

            protected override void OnUndo()
            {
                Model.DeleteDialog(_id);
            }
        }

        class CreateDialogPropertyAction : DialogsEditorAction
        {
            readonly string _id;
            readonly string _key;
            readonly string _value;

            public CreateDialogPropertyAction(EditorViewModel editor, string id, string key, string value)
                : base(editor, "Create dialog property")
            {
                _id = id;
                _key = key;
                _value = value;
            }

            protected override void OnExecute()
            {
                Model.SetDialogProperty(_id, _key, _value);
                Editor.SetSelectedProperty(_key);
            }

            protected override void OnUndo()
            {
                Model.DeleteDialogProperty(_id, _key);
                Editor.UpdatePropertiesButtons();
            }
        }
    }
}
