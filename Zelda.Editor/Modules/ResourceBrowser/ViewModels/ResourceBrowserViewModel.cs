using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Windows;
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
        public IMod Mod { get { return _modService.Mod; } }

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

        public void NewResourceElement(IModFile parent)
        {
            try
            {
                var path = parent.Path;
                var mod = _modService.Mod;
                var resources = mod.Resources;
                var resourceType = ResourceType.Map;
                var initialIdValue = "";

                if (mod.IsPotentialResourceElement(path, ref resourceType, ref initialIdValue))
                {
                    if (resources.Exists(resourceType, initialIdValue))
                        return;
                }
                else
                {
                    if (!mod.IsResourceDirectory(path, ref resourceType) &&
                        !mod.IsInResourceDirectory(path, ref resourceType))
                        return;

                    var resourceDir = mod.GetResourceDirectory(resourceType);
                    if (path != resourceDir)
                        initialIdValue = path.Substring(resourceDir.Length + 1) + '/';
                    else
                        initialIdValue = "";

                    var resourceTypeName = mod.Resources.GetFriendlyName(resourceType);
                    var dialog = new NewResourceElementViewModel(resourceTypeName, mod) { Id = initialIdValue };
                    dynamic settings = new ExpandoObject();
                    settings.Title = "Create resource";
                    settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (IoC.Get<IWindowManager>().ShowDialog(dialog, null, settings) != true)
                        return;

                    var elementId = dialog.Id;
                    var description = dialog.Description;

                    mod.CreateResourceElement(resourceType, elementId, description);

                    var createdPath = mod.GetResourceElementPath(resourceType, elementId);
                    if (mod.Exists(createdPath))
                    {
                        var file = ModFileBuilder.Create(mod, createdPath, parent);
                        SelectedModFile = file;
                        // TODO: Open file
                    }
                }
            }
            catch (Exception e)
            {
                e.ShowDialog();
            }
        }
    }
}
