using System;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Threading;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
{
    class EditorViewModel : PersistedDocument
    {
        readonly IMod _mod;
        string _languageId;
        string _description;

        public string LanguageId
        {
            get { return _languageId; }
            private set { this.SetProperty(ref _languageId, value); }
        }

        public string Description
        {
            get { return _description; }
            set { this.SetProperty(ref _description, value); }
        }

        public override Uri IconSource { get { return "/Resources/Icons/icon_resource_language.png".ToIconUri(); } }

        public EditorViewModel(IMod mod)
        {
            _mod = mod;
        }

        protected override Task DoLoad(string filePath)
        {
            var languageId = "";
            if (_mod.IsDialogsFile(filePath, ref languageId))
            {
                DisplayName = "Dialogs {0}".F(languageId);
                LanguageId = languageId;
                Description = _mod.Resources.GetDescription(Game.ResourceType.Language, languageId);
            }

            return TaskUtility.Completed;
        }
    }
}
