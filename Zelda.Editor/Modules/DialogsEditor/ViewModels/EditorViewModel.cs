using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Controls.ViewModels;
using Zelda.Editor.Core.Menus;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;
using Zelda.Editor.Modules.ContextMenus;
using Zelda.Editor.Modules.ContextMenus.Models;
using Zelda.Editor.Modules.DialogsEditor.Commands;
using Zelda.Editor.Modules.DialogsEditor.Models;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Editor.Modules.Mods.ViewModels;
using Zelda.Editor.Modules.ResourceSelector.Models;
using Zelda.Editor.Modules.ResourceSelector.ViewModels;
using Zelda.Editor.Modules.UndoRedo;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
{
    class EditorViewModel : EditorDocument,
        ICommandHandler<CreateDialogCommandDefinition>,
        ICommandHandler<DeleteDialogCommandDefinition>,
        ICommandHandler<SetDialogIdCommandDefinition>
    {
        readonly ContextMenuModel _contextMenu = new ContextMenuModel();

        string _languageId;
        KeyValuePair<string, Node>? _selectedItem;
        bool _isDialogPropertiesEnabled;
        string _dialogId;
        string _translationText;
        DialogPropertiesTable.Item _selectedPropertyItem;

        public IContextMenu ContextMenu { get { return _contextMenu; } }

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

        public KeyValuePair<string, Node>? SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (this.SetProperty(ref _selectedItem, value))
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

        Node SelectedDialogNode { get { return SelectedItem != null ? SelectedItem.Value.Value : null; } }
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
            get
            {
                if (DialogsModel.DialogExists(SelectedDialogId))
                    return DialogsModel.GetDialogText(SelectedDialogId);
                else
                    return null;
            }
            set { ChangeDialogText(value); }
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
            CloseConfirmMessage = "Dialogs '{0}' has been modified. Save changes?".F(languageId);

            DialogsModel = new DialogsModel(Mod, languageId);
            DialogsModel.DialogCreated += (_, e) => UpdateDialogViewIfDialogIdEquals(e);
            DialogsModel.DialogDeleted += (_, e) => UpdateDialogViewIfDialogIdEquals(e);
            DialogsModel.DialogIdChanged += (_, e) => UpdateDialogId();
            DialogsModel.DialogTextChanged += (_, e) => UpdateDialogText();

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

        public bool BuildContextMenu()
        {
            _contextMenu.Clear();

            var menuItems = new List<MenuItemDefinition>();
            menuItems.Add(new CommandMenuItemDefinition<CreateDialogCommandDefinition>(ContextMenuDefinitions.CreateMenuGroup, 0));

            if (DialogsModel.PrefixExists(SelectedDialogId))
            {
                menuItems.Add(new CommandMenuItemDefinition<SetDialogIdCommandDefinition>(ContextMenuDefinitions.SetIdMenuGroup, 0));
                menuItems.Add(new CommandMenuItemDefinition<DeleteDialogCommandDefinition>(ContextMenuDefinitions.DeleteMenuGroup, 0));
            }

            IoC.Get<IContextMenuBuilder>().BuildContextMenu(menuItems, _contextMenu);
            return _contextMenu.Count > 0;
        }

        void UpdateDialogViewIfDialogIdEquals(string id)
        {
            if (id == DialogId)
                UpdateDialogView();
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
                    TranslationSelector.SetSelectedId("");
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
            NotifyOfPropertyChange(() => DialogText);
        }

        void ChangeDialogText(string newText)
        {
            if (!DialogsModel.DialogExists(DialogId))
            {
                if (!newText.IsNullOrEmpty())
                    TryAction(new CreateDialogAction(this, DialogId, newText));
                return;
            }

            if (DialogText == newText)
                return;

            TryAction(new SetDialogTextAction(this, DialogId, newText));
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
            if (dialog.ShowDialog() != true)
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
            return SelectedDialogId != null && DialogsModel.PrefixExists(SelectedDialogId);
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
            var node = DialogsModel.DialogTree.Find(id);
            if (node != null)
                SelectedItem = node.Parent.Children.First(x => x.Value == node);
            else
                SelectedItem = null;
        }

        void SetSelectedProperty(string key)
        {
            SelectedPropertyItem = PropertiesTable.Items.FirstOrDefault(i => i.Key == key);
        }

        protected override Task OnSave()
        {
            DialogsModel.Save();
            return Task.FromResult(true);
        }

        public void OnPropertyCellEditEnding(DataGridCellEditEndingEventArgs param)
        {
            if (param.EditAction != DataGridEditAction.Commit || !CheckIsPropertyValueColumn(param.Column))
                return;

            var propertyItem = param.Row.DataContext as DialogPropertiesTable.Item;
            if (propertyItem == null)
                return;

            var newValue = (param.EditingElement as TextBox).Text;
            if (DialogsModel.GetDialogProperty(DialogId, propertyItem.Key) != newValue)
                ChangeDialogPropertyValue(propertyItem.Key, newValue);
        }

        static bool CheckIsPropertyValueColumn(DataGridColumn column)
        {
            var textColumn = column as DataGridTextColumn;
            if (textColumn == null)
                return false;

            var binding = textColumn.Binding as Binding;
            return (binding.Path.Path == "Value");
        }

        void ChangeDialogPropertyValue(string key, string value)
        {
            if (!DialogsModel.DialogExists(DialogId))
            {
                var properties = new Dictionary<string, string>();
                properties.Add(key, value);
                TryAction(new CreateDialogAction(this, DialogId, string.Empty, properties));
            }
            else if (!DialogsModel.DialogPropertyExists(DialogId, key))
                TryAction(new CreateDialogPropertyAction(this, DialogId, key, value));
            else if (value != DialogsModel.GetDialogProperty(DialogId, key))
                TryAction(new SetDialogPropertyValueAction(this, DialogId, key, value));
        }

        void ICommandHandler<CreateDialogCommandDefinition>.Update(Command command)
        {
            command.Enabled = CreateDialogCommand.CanExecute(null);
        }

        Task ICommandHandler<CreateDialogCommandDefinition>.Run(Command command)
        {
            CreateDialogCommand.Execute(null);
            return TaskUtility.Completed;
        }

        void ICommandHandler<DeleteDialogCommandDefinition>.Update(Command command)
        {
            command.Enabled = DeleteDialogCommand.CanExecute(null);
        }

        Task ICommandHandler<DeleteDialogCommandDefinition>.Run(Command command)
        {
            DeleteDialogCommand.Execute(null);
            return TaskUtility.Completed;
        }

        void ICommandHandler<SetDialogIdCommandDefinition>.Update(Command command)
        {
            command.Enabled = ChangeDialogIdCommand.CanExecute(null);
        }

        Task ICommandHandler<SetDialogIdCommandDefinition>.Run(Command command)
        {
            ChangeDialogIdCommand.Execute(null);
            return TaskUtility.Completed;
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

        class SetDialogPropertyValueAction : DialogsEditorAction
        {
            readonly string _id;
            readonly string _key;
            readonly string _oldValue;
            readonly string _newValue;

            public SetDialogPropertyValueAction(EditorViewModel editor, string id, string key, string newValue)
                : base(editor, "Change dialog property")
            {
                _id = id;
                _key = key;
                _oldValue = Model.GetDialogProperty(id, key);
                _newValue = newValue;
            }

            protected override void OnExecute()
            {
                Model.SetDialogProperty(_id, _key, _newValue);
                Editor.SetSelectedProperty(_key);
            }

            protected override void OnUndo()
            {
                Model.SetDialogProperty(_id, _key, _oldValue);
                Editor.SetSelectedProperty(_key);
            }
        }

        class SetDialogTextAction : DialogsEditorAction
        {
            readonly string _id;
            readonly string _oldText;
            readonly string _newText;

            public SetDialogTextAction(EditorViewModel editor, string id, string text)
                :base(editor, "Change dialog text")
            {
                _id = id;
                _oldText = Model.GetDialogText(id);
                _newText = text;
            }

            protected override void OnExecute()
            {
                Model.SetDialogText(_id, _newText);
                Editor.SetSelectedDialog(_id);
            }

            protected override void OnUndo()
            {
                Model.SetDialogText(_id, _oldText);
                Editor.SetSelectedDialog(_id);
            }
        }
    }
}
