using System;
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
        readonly IModService _modService;

        public override PaneLocation PreferredLocation { get { return PaneLocation.Left; } }
        public override double PreferredWidth { get { return 400.0; } }
        public IEnumerable<IModFile> ModRootFiles { get; private set; }

        [ImportingConstructor]
        public ResourceBrowserViewModel(IModService modService)
        {
            _modService = modService;
            DisplayName = "Resource Browser";

            _modService.Loaded += ModService_Loaded;
            _modService.Unloaded += ModService_Unloaded;
        }

        void ModService_Loaded(object sender, EventArgs e)
        {
            ModRootFiles = _modService.Mod.RootDirectory.Yield();
            NotifyOfPropertyChange(() => ModRootFiles);
        }

        void ModService_Unloaded(object sender, EventArgs e)
        {
            ModRootFiles = null;
            NotifyOfPropertyChange(() => ModRootFiles);
        }
    }
}
