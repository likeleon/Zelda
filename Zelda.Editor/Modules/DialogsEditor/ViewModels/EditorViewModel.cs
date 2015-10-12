﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Controls.ViewModels;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.DialogsEditor.Models;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Editor.Modules.Mods.ViewModels;
using Zelda.Editor.Modules.ResourceSelector.Models;
using Zelda.Editor.Modules.ResourceSelector.ViewModels;
using Zelda.Editor.Modules.UndoRedo;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
{
    class EditorViewModel : EditorDocument
    {
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

                return Mod.Resources.GetDescription(ResourceType.Language, LanguageId);
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
        public RelayCommand CreateDialogCommand { get; private set; }
        public RelayCommand ChangeDialogIdCommand { get; private set; }
        public RelayCommand DeleteDialogCommand { get; private set; }
        public RelayCommand CreatePropertyCommand { get; private set; }
        public RelayCommand SetPropertyKeyCommand { get; private set; }
        public RelayCommand DeletePropertyCommand { get; private set; }


        public EditorViewModel(IMod mod, string filePath)
            : base(mod, filePath)
        {
            Resources.ElementDescriptionChanged += (_, e) => NotifyOfPropertyChange(() => Description);

            var languageId = "";
            if (!Mod.IsDialogsFile(filePath, ref languageId))
                throw new InvalidOperationException("Path is not dialogs file: {0}".F(filePath));

            Title = "Dialogs {0}".F(languageId);
            LanguageId = languageId;

            DialogsModel = new DialogsModel(Mod, languageId);
            DialogsModel.DialogIdChanged += (_, e) => UpdateDialogId();

            PropertiesTable = new DialogPropertiesTable(DialogsModel);

            TranslationSelector = new SelectorViewModel(Mod, ResourceType.Language);
            TranslationSelector.RemoveId(languageId);
            TranslationSelector.AddSpecialValue("", "<No language>", 0);
            TranslationSelector.SetSelectedId("");
            TranslationSelector.SelectedItemChanged += TranslationSelector_SelectedItemChanged;

            RefreshTranslationCommand = new RelayCommand(_ => RefreshTranslation());
            CreateDialogCommand = new RelayCommand(_ => CreateDialog());
            ChangeDialogIdCommand = new RelayCommand(_ => ChangeDialogId(), _ => CanExecuteDialogCommand());
            DeleteDialogCommand = new RelayCommand(_ => DeleteDialog(), _ => CanExecuteDialogCommand());
            CreatePropertyCommand = new RelayCommand(_ => CreateProperty());
            SetPropertyKeyCommand = new RelayCommand(_ => ChangeDialogPropertyKey(),
                                                     _ => SelectedDialogPropertyExists());
            DeletePropertyCommand = new RelayCommand(_ => DeleteDialogProperty(),
                                                     _ => SelectedDialogPropertyExists());
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
            ChangeDialogIdCommand.RaiseCanExecuteChanged();
            DeleteDialogCommand.RaiseCanExecuteChanged();            
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

        void CreateDialog()
        {
            var id = TextInputViewModel.GetText("New dialog", "New dialog id:", SelectedDialogId);
            if (id == null)
                return;

            if (!DialogsModel.IsValidId(id))
            {
                "Invalid dialog id: {0}".F(id).ShowErrorDialog();
                return;
            }

            if (DialogsModel.DialogExists(id))
            {
                "Dialog '{0}' already exists".F(id).ShowErrorDialog();
                return;
            }

            TryAction(new CreateDialogAction(this, id, ""));
        }

        void ChangeDialogId()
        {
            if (SelectedDialogId == null)
                return;

            var oldId = SelectedDialogId;
            var prefixIds = DialogsModel.GetIds(oldId);
            if (prefixIds.Length <= 0)
                return;

            var exists = false;
            var isPrefix = true;
            if (DialogsModel.DialogExists(oldId))
            {
                exists = true;
                isPrefix = prefixIds.Length > 1;
            }

            var dialog = new ChangeDialogIdViewModel(DialogsModel, oldId, isPrefix, isPrefix && exists);
            if (IoC.Get<IWindowManager>().ShowDialog(dialog) != true)
                return;

            var newId = dialog.DialogId;
            if (newId == oldId)
                return;

            if (dialog.IsPrefix)
                TryAction(new SetIdPrefixAction(this, oldId, newId));
            else
                TryAction(new SetDialogIdAction(this, oldId, newId));
        }

        void DeleteDialog()
        {
            if (SelectedDialogId == null)
                return;

            var id = SelectedDialogId;
            if (!DialogsModel.PrefixExists(id))
                return;

            if (DialogsModel.DialogExists(id))
            {
                TryAction(new DeleteDialogAction(this, id));
                return;
            }

            var question = "Do you really want to delete all dialog prefixed by '{0}'?".F(id);
            if (!question.AskYesNo("Delete confirmation"))
                return;

            TryAction(new DeleteDialogsAction(this, id));
        }

        bool CanExecuteDialogCommand()
        {
            return SelectedDialogId != null &&
                   DialogsModel.PrefixExists(SelectedDialogId);
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
            var oldKey = SelectedPropertyKey;
            if (oldKey == null || !DialogsModel.DialogPropertyExists(SelectedDialogId, oldKey))
                return;

            var newKey = TextInputViewModel.GetText("Change dialog property key",
                "Change the key of the property '{0}':".F(oldKey), oldKey);
            if (newKey == null || newKey == oldKey)
                return;

            if (newKey.IsNullOrEmpty())
            {
                "The property key cannot be empty".ShowErrorDialog();
                return;
            }

            TryAction(new SetDialogPropertyKeyAction(this, SelectedDialogId, oldKey, newKey));
        }

        void DeleteDialogProperty()
        {
            if (!DialogsModel.DialogPropertyExists(SelectedDialogId, SelectedPropertyKey))
                return;

            TryAction(new DeleteDialogPropertyAction(this, SelectedDialogId, SelectedPropertyKey));
        }

        void SetSelectedDialog(string id)
        {
            SelectedDialogNode = DialogsModel.FindNode(id);
        }

        void SetSelectedProperty(string key)
        {
            SelectedPropertyItem = PropertiesTable.Items.FirstOrDefault(i => i.Key == key);
        }

        protected override Task OnSave()
        {
            DialogsModel.Save();
            var task = Task.FromResult(true);
            return task;
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

            public CreateDialogAction(EditorViewModel editor, string id, string text, Dictionary<string, string> properties = null)
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

        class SetDialogPropertyKeyAction : DialogsEditorAction
        {
            readonly string _id;
            readonly string _oldKey;
            readonly string _newKey;
            readonly string _value;

            public SetDialogPropertyKeyAction(EditorViewModel editor, string id, string oldKey, string newKey)
                : base(editor, "Create dialog property key")
            {
                _id = id;
                _oldKey = oldKey;
                _newKey = newKey;
                _value = Model.GetDialogProperty(id, oldKey);
            }

            protected override void OnExecute()
            {
                Model.DeleteDialogProperty(_id, _oldKey);
                Model.SetDialogProperty(_id, _newKey, _value);
                Editor.SetSelectedProperty(_newKey);
            }

            protected override void OnUndo()
            {
                Model.DeleteDialogProperty(_id, _newKey);
                Model.SetDialogProperty(_id, _oldKey, _value);
                Editor.SetSelectedProperty(_oldKey);
            }
        }

        class DeleteDialogPropertyAction : DialogsEditorAction
        {
            readonly string _id;
            readonly string _key;
            readonly string _value;

            public DeleteDialogPropertyAction(EditorViewModel editor, string id, string key)
                : base(editor, "Delete dialog property")
            {
                _id = id;
                _key = key;
                _value = Model.GetDialogProperty(id, key);
            }

            protected override void OnExecute()
            {
                Model.DeleteDialogProperty(_id, _key);
                Editor.UpdatePropertiesButtons();
            }

            protected override void OnUndo()
            {
                Model.SetDialogProperty(_id, _key, _value);
                Editor.SetSelectedProperty(_key);
            }
        }

        class SetIdPrefixAction : DialogsEditorAction
        {
            readonly string _oldPrefix;
            readonly string _newPrefix;
            List<Tuple<string, string>> _editedIds;

            public SetIdPrefixAction(EditorViewModel editor, string oldPrefix, string newPrefix)
                :base(editor, "Change dialog id prefix")
            {
                _oldPrefix = oldPrefix;
                _newPrefix = newPrefix;
            }

            protected override void OnExecute()
            {
                _editedIds = Model.SetDialogIdPrefix(_oldPrefix, _newPrefix);
                if (_editedIds.Count > 0)
                    Editor.SetSelectedDialog(_editedIds.First().Item2);
            }

            protected override void OnUndo()
            {
                _editedIds.Do(tuple => Model.SetDialogId(tuple.Item1, tuple.Item2));
                if (_editedIds.Count > 0)
                    Editor.SetSelectedDialog(_editedIds.First().Item1);
            }
        }

        class SetDialogIdAction : DialogsEditorAction
        {
            readonly string _oldId;
            readonly string _newId;

            public SetDialogIdAction(EditorViewModel editor, string id, string newId)
                : base(editor, "Change dialog id")
            {
                _oldId = id;
                _newId = newId;
            }

            protected override void OnExecute()
            {
                Model.SetDialogId(_oldId, _newId);
                Editor.SetSelectedDialog(_newId);
            }

            protected override void OnUndo()
            {
                Model.SetDialogId(_newId, _oldId);
                Editor.SetSelectedDialog(_oldId);
            }
        }

        class DeleteDialogAction : DialogsEditorAction
        {
            readonly string _id;
            readonly string _text;
            readonly Dictionary<string, string> _properties;

            public DeleteDialogAction(EditorViewModel editor, string id)
                : base(editor, "Delete dialog")
            {
                _id = id;
                _text = Model.GetDialogText(id);
                _properties = Model.GetDialogProperties(_id).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            protected override void OnExecute()
            {
                Model.DeleteDialog(_id);
            }

            protected override void OnUndo()
            {
                Model.CreateDialog(_id, _text, _properties);
                Editor.SetSelectedDialog(_id);
            }
        }

        class DeleteDialogsAction : DialogsEditorAction
        {
            readonly string _prefix;
            List<Tuple<string, DialogData>> _removedDialogs;

            public DeleteDialogsAction(EditorViewModel editor, string prefix)
                : base(editor, "Delete dialogs")
            {
                _prefix = prefix;
            }

            protected override void OnExecute()
            {
                _removedDialogs = Model.DeletePrefix(_prefix);
            }

            protected override void OnUndo()
            {
                _removedDialogs.Do(tuple => Model.CreateDialog(tuple.Item1, tuple.Item2));
                if (_removedDialogs.Count > 0)
                    Editor.SetSelectedDialog(_removedDialogs.First().Item1);
            }
        }
    }
}
