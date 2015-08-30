using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser.ViewModels
{
    [Export(typeof(IResourceBrowser))]
    class ResourceBrowserViewModel : Tool, IResourceBrowser
    {
        readonly IModService _modService;
        readonly ICommandService _commandService;
        IModFile _selectedModFile;

        public override PaneLocation PreferredLocation { get { return PaneLocation.Left; } }
        public override double PreferredWidth { get { return 400.0; } }
        public IEnumerable<IModFile> ModRootFiles { get; private set; }
        public IModFile SelectedModFile
        {
            get { return _selectedModFile; }
            set { this.SetProperty(ref _selectedModFile, value); }
        }

        [ImportingConstructor]
        public ResourceBrowserViewModel(IModService modService, ICommandService commandService)
        {
            _modService = modService;
            _commandService = commandService;

            DisplayName = "Resource Browser";

            _modService.Loaded += ModService_Loaded;
            _modService.Unloaded += ModService_Unloaded;
        }

        void ModService_Loaded(object sender, EventArgs e)
        {
            ModRootFiles = ModFileBuilder.Build(_modService.Mod).Yield();
            NotifyOfPropertyChange(() => ModRootFiles);
        }

        void ModService_Unloaded(object sender, EventArgs e)
        {
            ModRootFiles = null;
            NotifyOfPropertyChange(() => ModRootFiles);
        }
    }
}
