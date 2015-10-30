using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Controls;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.DialogsEditor.Models;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Editor.Modules.Mods.ViewModels;
using Zelda.Editor.Modules.ResourceSelector.Models;
using Zelda.Editor.Modules.ResourceSelector.ViewModels;
using Zelda.Editor.Modules.StringsEditor.Models;
using Zelda.Editor.Modules.UndoRedo;
using Zelda.Game;

namespace Zelda.Editor.Modules.StringsEditor.ViewModels
{
    class EditorViewModel : EditorDocument
    {
        string _languageId;
        KeyValuePair<string, Node>? _selectedString;

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

        public KeyValuePair<string, Node>? SelectedString
        {
            get { return _selectedString; }
            set
            {
                if (this.SetProperty(ref _selectedString, value))
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

        string SelectedStringKey { get { return SelectedString != null ? SelectedString.Value.Key : string.Empty; } }

        public EditorViewModel(IMod mod, string filePath)
            :base(mod, filePath)
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
            //SetKeyCommand.RaiseCanExecuteChanged();
            //DeleteCommand.RaiseCanExecuteChanged();
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

        void SetSelectedString(string key)
        {
            var node = StringsModel.StringTree.Find(key);
            if (node != null)
                SelectedString = node.Parent.Children.First(x => x.Value == node);
            else
                SelectedString = null;
        }

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
                Editor.SetSelectedString(_key);
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
                Editor.SetSelectedString(_key);
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
                Editor.SetSelectedString(_key);
            }

            protected override void OnUndo()
            {
                Model.SetString(_key, _oldValue);
                Editor.SetSelectedString(_key);
            }
        }
    }
}
