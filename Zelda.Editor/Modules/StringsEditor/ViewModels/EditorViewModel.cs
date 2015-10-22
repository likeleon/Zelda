using System;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Editor.Modules.Mods.ViewModels;
using Zelda.Editor.Modules.ResourceSelector.Models;
using Zelda.Editor.Modules.ResourceSelector.ViewModels;
using Zelda.Game;

namespace Zelda.Editor.Modules.StringsEditor.ViewModels
{
    class EditorViewModel : EditorDocument
    {
        string _languageId;

        public string LanguageId
        {
            get { return _languageId; }
            private set
            {
                if (this.SetProperty(ref _languageId, value))
                    NotifyOfPropertyChange(() => Description);
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

            TranslationSelector = new SelectorViewModel(Mod, ResourceType.Language);
            TranslationSelector.RemoveId(languageId);
            TranslationSelector.AddSpecialValue("", "<No language>", 0);
            TranslationSelector.SetSelectedId("");
            TranslationSelector.SelectedItemChanged += TranslationSelector_SelectedItemChanged;
        }

        void TranslationSelector_SelectedItemChanged(object sender, Item e)
        {
            throw new NotImplementedException();
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

        protected override Task OnSave()
        {
            return Task.FromResult(true);
        }
    }
}
