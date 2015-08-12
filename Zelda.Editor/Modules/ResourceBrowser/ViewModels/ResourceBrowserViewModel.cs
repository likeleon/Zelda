using System.Collections.Generic;
using System.ComponentModel.Composition;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser.ViewModels
{
    [Export(typeof(IResourceBrowser))]
    class ResourceBrowserViewModel : Tool, IResourceBrowser
    {
        public IMod Mod { get; private set; }

        public override PaneLocation PreferredLocation { get { return PaneLocation.Left; } }
        public override double PreferredWidth { get { return 400.0; } }
        public IEnumerable<IModFile> ModRootFiles { get { return Mod.RootDirectory.Yield(); } }

        [ImportingConstructor]
        public ResourceBrowserViewModel(IMod mod)
        {
            Mod = mod;
            DisplayName = "Resource Browser";

            Mod.Loaded += Mod_Loaded;
            Mod.Unloaded += Mod_Unloaded;
        }

        void Mod_Loaded(object sender, System.EventArgs e)
        {
            NotifyOfPropertyChange(() => ModRootFiles);
        }

        void Mod_Unloaded(object sender, System.EventArgs e)
        {
            NotifyOfPropertyChange(() => ModRootFiles);
        }
    }
}
