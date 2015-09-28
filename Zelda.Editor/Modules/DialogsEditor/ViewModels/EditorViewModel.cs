using System;
using System.Linq;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;
using Zelda.Editor.Modules.DialogsEditor.Models;
using Zelda.Editor.Modules.ResourceSelector;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
{
    class EditorViewModel : PersistedDocument
    {
        readonly IMod _mod;
        string _languageId;
        Node _selectedNode;

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

        public Node SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (this.SetProperty(ref _selectedNode, value))
                {
                    NotifyOfPropertyChange(() => DialogExists);
                    NotifyOfPropertyChange(() => DialogId);
                    NotifyOfPropertyChange(() => DialogText);
                }
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

        public bool DialogExists { get { return SelectedNode != null && DialogsModel.DialogExists(SelectedNode); } }
        public string DialogId { get { return DialogExists ? SelectedNode.Key : ""; } }
        public string DialogText
        {
            get { return DialogExists ? DialogsModel.GetDialogText(SelectedNode) : ""; }
            set { /* TODO: SetDialogTextCommand */ }
        }
        public Selector TranslationSelector { get; private set; }

        Core.Mods.ModResources Resources { get { return _mod.Resources; } }

        public EditorViewModel(IMod mod)
        {
            _mod = mod;
            Resources.ElementDescriptionChanged += (_, e) => NotifyOfPropertyChange(() => Description);
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

            TranslationSelector = new Selector(_mod, ResourceType.Language);
            TranslationSelector.RemoveId(languageId);
            TranslationSelector.AddSpecialValue("", "<No language>", 0);
            TranslationSelector.SetSelectedId("");
            NotifyOfPropertyChange(() => TranslationSelector);

            return TaskUtility.Completed;
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
    }
}
