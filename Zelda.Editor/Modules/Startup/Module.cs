using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using Zelda.Editor.Core;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Editor.Modules.Mods.Services;
using Zelda.Editor.Modules.Output;
using Zelda.Editor.Modules.ResourceBrowser;
using Zelda.Game;

namespace Zelda.Editor.Modules.Startup
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        readonly IModService _modService;
        readonly IOutput _output;

        public override IEnumerable<Type> DefaultTools { get { yield return typeof(IResourceBrowser); } }

        [ImportingConstructor]
        public Module(IModService modService, IOutput output)
        {
            _modService = modService;
            _output = output;
        }

        public override void Initialize()
        {
            _modService.Loaded += ModService_Loaded;
            _modService.Unloaded += ModService_Unloaded;

            UpdateTitle();

            Shell.StatusBar.AddItem("Ready", new GridLength(1, GridUnitType.Star));
            _output.AppendLine("Started up");
        }

        public override void PostInitialize()
        {
            var sampleModPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\mods\alttp"));
            _modService.Load(sampleModPath);
        }

        void ModService_Loaded(object sender, IMod loadedMod)
        {
            UpdateTitle();
        }

        void UpdateTitle()
        {
            var version = ZeldaVersion.Version;
            var title = "Zelda Mod Editor {0}".F(version);
            if (_modService.IsLoaded)
                title = _modService.Mod.Name + " - " + title;

            MainWindow.Title = title;
        }

        void ModService_Unloaded(object sender, IMod unloadedMod)
        {
            UpdateTitle();
        }
    }
}
