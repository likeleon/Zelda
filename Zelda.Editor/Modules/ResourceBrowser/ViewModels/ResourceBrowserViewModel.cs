using System.ComponentModel.Composition;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.ResourceBrowser.ViewModels
{
    [Export(typeof(IResourceBrowser))]
    class ResourceBrowserViewModel : Tool, IResourceBrowser
    {
        public IMod Mod { get; private set; }

        public override PaneLocation PreferredLocation { get { return PaneLocation.Left; } }

        [ImportingConstructor]
        public ResourceBrowserViewModel(IMod mod)
        {
            Mod = mod;
            DisplayName = "Resource Browser";
        }
    }
}
