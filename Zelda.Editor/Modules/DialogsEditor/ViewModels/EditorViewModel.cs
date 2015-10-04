using System;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;
using Zelda.Editor.Modules.DialogsEditor.Models;
using Zelda.Editor.Modules.ResourceSelector.Models;
using Zelda.Editor.Modules.ResourceSelector.ViewModels;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
{
    class EditorViewModel : PersistedDocument
    {
        readonly IMod _mod;
        string _languageId;
        Node _selectedNode;
        bool _isDialogPropertiesEnabled;
        string _dialogId;
        string _dialogText;
        string _translationText;

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

        public Node SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (this.SetProperty(ref _selectedNode, value))
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

        string SelectedNodeKey { get { return SelectedNode != null ? SelectedNode.Key : null; } }

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

        public RelayCommand RefreshTranslationCommand { get; private set; }

        Core.Mods.ModResources Resources { get { return _mod.Resources; } }

        public EditorViewModel(IMod mod)
        {
            _mod = mod;
            Resources.ElementDescriptionChanged += (_, e) => NotifyOfPropertyChange(() => Description);

            RefreshTranslationCommand = new RelayCommand(_ => RefreshTranslation());
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
            var exists = DialogsModel.DialogExists(SelectedNodeKey);
            var hasTranslation = DialogsModel.TranslatedDialogExists(SelectedNodeKey);
            IsDialogPropertiesEnabled = exists || hasTranslation;

            UpdateDialogId();
            UpdateDialogText();
            UpdateTranslationText();

            PropertiesTable.DialogId = SelectedNodeKey;
        }

        void UpdateDialogId()
        {
            if (DialogsModel.DialogExists(SelectedNodeKey) || DialogsModel.TranslatedDialogExists(SelectedNodeKey))
                DialogId = SelectedNodeKey;
            else
                DialogId = null;
        }

        void UpdateDialogText()
        {
            if (DialogsModel.DialogExists(SelectedNodeKey))
                DialogText = DialogsModel.GetDialogText(SelectedNodeKey);
            else
                DialogText = null;
        }

        void UpdateTranslationText()
        {
            if (DialogsModel.TranslatedDialogExists(SelectedNodeKey))
                TranslationText = DialogsModel.GetTranslatedDialogText(SelectedNodeKey);
            else
                TranslationText = null;
        }

        void RefreshTranslation()
        {
            if (DialogsModel.TranslationId == null)
                return;

            DialogsModel.ReloadTranslation();
            UpdateTranslationText();
        }
    }
}
