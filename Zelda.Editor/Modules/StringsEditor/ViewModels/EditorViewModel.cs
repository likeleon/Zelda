using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Controls;
using Zelda.Editor.Core.Menus;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;
using Zelda.Editor.Modules.ContextMenus;
using Zelda.Editor.Modules.ContextMenus.Models;
using Zelda.Editor.Modules.DialogsEditor.Models;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Editor.Modules.Mods.ViewModels;
using Zelda.Editor.Modules.ResourceSelector.Models;
using Zelda.Editor.Modules.ResourceSelector.ViewModels;
using Zelda.Editor.Modules.StringsEditor.Commands;
using Zelda.Editor.Modules.StringsEditor.Models;
using Zelda.Editor.Modules.UndoRedo;
using Zelda.Game;

namespace Zelda.Editor.Modules.StringsEditor.ViewModels
{
    class EditorViewModel : EditorDocument,
        ICommandHandler<CreateStringCommandDefinition>,
        ICommandHandler<DeleteStringCommandDefinition>,
        ICommandHandler<SetStringKeyCommandDefinition>
    {
        readonly ContextMenuModel _contextMenu = new ContextMenuModel();

        string _languageId;
        KeyValuePair<string, Node>? _selectedItem;

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

        public StringsModel StringsModel { get; private set; }

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

        public RelayCommand RefreshTranslationCommand { get; private set; }
        public RelayCommand CreateStringCommand { get; private set; }
        public RelayCommand ChangeStringKeyCommand { get; private set; }
        public RelayCommand DeleteStringCommand { get; private set; }

        Node SelectedStringNode { get { return SelectedItem != null ? SelectedItem.Value.Value : null; } }
        string SelectedStringKey { get { return SelectedStringNode != null ? SelectedStringNode.Key : string.Empty; } }

        public EditorViewModel(IMod mod, string filePath)
            : base(mod, filePath)
        {
            Resources.ElementDescriptionChanged += (_, e) => NotifyOfPropertyChange(() => Description);

            var languageId = "";
            if (!Mod.IsStringsFile(filePath, ref languageId))
                throw new InvalidOperationException("Path is not strings file: {0}".F(filePath));

            Title = "Strings {0}".F(languageId);
            LanguageId = languageId;
            CloseConfirmMessage = "Strings '{0}' has been modified. Save changes?".F(languageId);

            StringsModel = new StringsModel(Mod, languageId);

            TranslationSelector = new SelectorViewModel(Mod, ResourceType.Language);
            TranslationSelector.RemoveId(languageId);
            TranslationSelector.AddSpecialValue("", "<No language>", 0);
            TranslationSelector.SetSelectedId("");
            TranslationSelector.SelectedItemChanged += TranslationSelector_SelectedItemChanged;

            RefreshTranslationCommand = new RelayCommand(_ => RefreshTranslation());
            CreateStringCommand = new RelayCommand(_ => CreateString());
            ChangeStringKeyCommand = new RelayCommand(_ => ChangeStringKey(), _ => CanExecuteStringCommand());
            DeleteStringCommand = new RelayCommand(_ => DeleteString(), _ => CanExecuteStringCommand());
        }

        public bool BuildContextMenu()
        {
            _contextMenu.Clear();

            var menuItems = new List<MenuItemDefinition>();
            menuItems.Add(new CommandMenuItemDefinition<CreateStringCommandDefinition>(ContextMenuDefinitions.CreateMenuGroup, 0));

            if (StringsModel.PrefixExists(SelectedStringKey))
            {
                menuItems.Add(new CommandMenuItemDefinition<SetStringKeyCommandDefinition>(ContextMenuDefinitions.SetIdMenuGroup, 0));
                menuItems.Add(new CommandMenuItemDefinition<DeleteStringCommandDefinition>(ContextMenuDefinitions.DeleteMenuGroup, 0));
            }

            IoC.Get<IContextMenuBuilder>().BuildContextMenu(menuItems, _contextMenu);
            return _contextMenu.Count > 0;
        }

        void TranslationSelector_SelectedItemChanged(object sender, Item e)
        {
            var newLanguage = e as ElementItem;
            if (newLanguage == null)
            {
                StringsModel.ClearTranslation();
                return;
            }

            try
            {
                StringsModel.SetTranslationId(newLanguage.Id);
            }
            catch (Exception ex)
            {
                ex.ShowDialog();
                TranslationSelector.SetSelectedId("");
            }
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
            ChangeStringKeyCommand.RaiseCanExecuteChanged();
            DeleteStringCommand.RaiseCanExecuteChanged();
        }

        void RefreshTranslation()
        {
            if (StringsModel.TranslationId == null)
                return;

            StringsModel.ReloadTranslation();
        }

        void CreateString()
        {
            var dialog = new NewStringDialogViewModel(StringsModel, SelectedStringKey);
            if (dialog.ShowDialog() == true)
                TryAction(new CreateStringAction(this, dialog.StringKey, dialog.StringValue));
        }

        void ChangeStringKey()
        {
            if (!CanExecuteStringCommand())
                return;

            var oldKey = SelectedStringKey;
            var prefixKeys = StringsModel.GetKeys(oldKey);
            if (prefixKeys.Length <= 0)
                return;

            var exists = false;
            var isPrefix = true;
            if (StringsModel.StringExists(oldKey))
            {
                exists = true;
                isPrefix = prefixKeys.Length > 1;
            }

            var dialog = new ChangeStringKeyViewModel(StringsModel, oldKey, isPrefix, isPrefix && exists);
            if (dialog.ShowDialog() != true)
                return;

            var newKey = dialog.StringKey;
            if (newKey == oldKey)
                return;

            if (dialog.IsPrefix)
                TryAction(new SetKeyPrefixAction(this, oldKey, newKey));
            else
                TryAction(new SetStringKeyAction(this, oldKey, newKey));
        }

        bool CanExecuteStringCommand()
        {
            return SelectedStringKey != null && StringsModel.PrefixExists(SelectedStringKey);
        }

        void DeleteString()
        {
            var key = SelectedStringKey;
            if (key == null)
                return;

            if (!StringsModel.PrefixExists(key))
                return;

            if (StringsModel.StringExists(key))
            {
                TryAction(new DeleteStringAction(this, key));
                return;
            }

            var question = "Do you really want to delete all strings prefixed by '{0}'".F(key);
            if (!question.AskYesNo("Delete confirmation"))
                return;

            TryAction(new DeleteStringAction(this, key));
        }

        protected override Task OnSave()
        {
            StringsModel.Save();
            return Task.FromResult(true);
        }

        public void SetStringValue(StringNode node, EditableTextBlockEditEndingEventArgs args)
        {
            var key = node.Key;
            var value = args.NewValue;

            if (!StringsModel.StringExists(key))
            {
                if (!value.IsNullOrEmpty())
                    TryAction(new CreateStringAction(this, key, value));
            }
            else
            {
                if (value.IsNullOrEmpty())
                    TryAction(new DeleteStringAction(this, key));
                else if (value != StringsModel.GetString(key))
                    TryAction(new SetStringValueAction(this, key, value));
            }
        }

        void SetSelectedKey(string key)
        {
            var node = StringsModel.StringTree.Find(key);
            if (node != null)
                SelectedItem = node.Parent.Children.First(x => x.Value == node);
            else
                SelectedItem = null;
        }

        #region Command Handlers
        void ICommandHandler<CreateStringCommandDefinition>.Update(Command command)
        {
            command.Enabled = CreateStringCommand.CanExecute(null);
        }

        Task ICommandHandler<CreateStringCommandDefinition>.Run(Command command)
        {
            CreateStringCommand.Execute(null);
            return TaskUtility.Completed;
        }

        void ICommandHandler<DeleteStringCommandDefinition>.Update(Command command)
        {
            command.Enabled = DeleteStringCommand.CanExecute(null);
        }

        Task ICommandHandler<DeleteStringCommandDefinition>.Run(Command command)
        {
            DeleteStringCommand.Execute(null);
            return TaskUtility.Completed;
        }

        void ICommandHandler<SetStringKeyCommandDefinition>.Update(Command command)
        {
            command.Enabled = ChangeStringKeyCommand.CanExecute(null);
        }

        Task ICommandHandler<SetStringKeyCommandDefinition>.Run(Command command)
        {
            ChangeStringKeyCommand.Execute(null);
            return TaskUtility.Completed;
        }
        #endregion

        abstract class StringsEditorAction : IUndoableAction
        {
            public EditorViewModel Editor { get; private set; }
            public StringsModel Model { get; private set; }

            public string Name { get; private set; }

            public void Execute()
            {
                OnExecute();
            }

            public void Undo()
            {
                OnUndo();
            }

            public StringsEditorAction(EditorViewModel editor, string name)
            {
                Editor = editor;
                Name = name;
                Model = editor.StringsModel;
            }

            protected abstract void OnExecute();
            protected abstract void OnUndo();
        }

        class CreateStringAction : StringsEditorAction
        {
            readonly string _key;
            readonly string _value;

            public CreateStringAction(EditorViewModel editor, string key, string value)
                : base(editor, "Create string")
            {
                _key = key;
                _value = value;
            }

            protected override void OnExecute()
            {
                Model.CreateString(_key, _value);
                Editor.SetSelectedKey(_key);
            }

            protected override void OnUndo()
            {
                Model.DeleteString(_key);
            }
        }

        class DeleteStringAction : StringsEditorAction
        {
            readonly string _key;
            readonly string _value;

            public DeleteStringAction(EditorViewModel editor, string key)
                : base(editor, "Delete string")
            {
                _key = key;
                _value = Model.GetString(key);
            }

            protected override void OnExecute()
            {
                Model.DeleteString(_key);
            }

            protected override void OnUndo()
            {
                Model.CreateString(_key, _value);
                Editor.SetSelectedKey(_key);
            }
        }

        class SetStringValueAction : StringsEditorAction
        {
            readonly string _key;
            readonly string _oldValue;
            readonly string _newValue;

            public SetStringValueAction(EditorViewModel editor, string key, string value)
                : base(editor, "Change string value")
            {
                _key = key;
                _oldValue = Model.GetString(_key);
                _newValue = value;
            }

            protected override void OnExecute()
            {
                Model.SetString(_key, _newValue);
                Editor.SetSelectedKey(_key);
            }

            protected override void OnUndo()
            {
                Model.SetString(_key, _oldValue);
                Editor.SetSelectedKey(_key);
            }
        }

        class SetKeyPrefixAction : StringsEditorAction
        {
            readonly string _newPrefix;
            readonly string _oldPrefix;
            List<Tuple<string, string>> _editedKeys;

            public SetKeyPrefixAction(EditorViewModel editor, string oldPrefix, string newPrefix)
                : base(editor, "Change string key prefix")
            {
                _oldPrefix = oldPrefix;
                _newPrefix = newPrefix;
            }

            protected override void OnExecute()
            {
                _editedKeys = Model.SetStringKeyPrefix(_oldPrefix, _newPrefix);
                if (_editedKeys.Count > 0)
                    Editor.SetSelectedKey(_editedKeys.First().Item1);
            }

            protected override void OnUndo()
            {
                _editedKeys.Do(tuple => Model.SetStringKey(tuple.Item1, tuple.Item2));
                if (_editedKeys.Count > 0)
                    Editor.SetSelectedKey(_editedKeys.First().Item1);
            }
        }

        class SetStringKeyAction : StringsEditorAction
        {
            readonly string _oldKey;
            readonly string _newKey;

            public SetStringKeyAction(EditorViewModel editor, string oldKey, string newKey)
                : base(editor,"Change string key")
            {
                _oldKey = oldKey;
                _newKey = newKey;
            }

            protected override void OnExecute()
            {
                Model.SetStringKey(_oldKey, _newKey);
                Editor.SetSelectedKey(_newKey);
            }

            protected override void OnUndo()
            {
                Model.SetStringKey(_newKey, _oldKey);
                Editor.SetSelectedKey(_oldKey);
            }
        }
    }
}
