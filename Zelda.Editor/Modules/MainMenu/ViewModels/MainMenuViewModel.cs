using System.ComponentModel.Composition;
using Zelda.Editor.Modules.MainMenu.Models;

namespace Zelda.Editor.Modules.MainMenu.ViewModels
{
    [Export(typeof(IMenu))]
    public class MainMenuViewModel : MenuModel, IPartImportsSatisfiedNotification
    {
        readonly IMenuBuilder _menuBuilder;

        [ImportingConstructor]
        public MainMenuViewModel(IMenuBuilder menuBuilder)
        {
            _menuBuilder = menuBuilder;
        }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            _menuBuilder.BuildMenuBar(MenuDefinitions.MainMenuBar, this);
        }
    }
}
