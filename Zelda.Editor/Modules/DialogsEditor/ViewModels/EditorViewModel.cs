using System;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
{
    class EditorViewModel : PersistedDocument
    {
        readonly IMod _mod;
        string _languageId;

        public string LanguageId
        {
            get { return _languageId; }
            private set { this.SetProperty(ref _languageId, value); }
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

        public override Uri IconSource { get { return "/Resources/Icons/icon_resource_language.png".ToIconUri(); } }

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
            NotifyOfPropertyChange(() => Description);
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
