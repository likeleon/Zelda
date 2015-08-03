using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Modules.ErrorList;
using Zelda.Editor.Modules.Output;
using Zelda.Editor.Modules.Progress;
using Zelda.Editor.Modules.ResourceBrowser;
using Zelda.Game;

namespace Zelda.Editor.Modules.Startup
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        readonly IMod _mod;
        readonly IOutput _output;

        public override IEnumerable<Type> DefaultTools
        {
            get 
            {
                yield return typeof(IOutput);
                yield return typeof(IErrorList);
                yield return typeof(IProgressTool); 
                yield return typeof(IResourceBrowser);
            }
        }

        [ImportingConstructor]
        public Module(IMod mod, IOutput output)
        {
            _mod = mod;
            _output = output;
        }

        public override void Initialize()
        {
            _mod.Loaded += OnModLoaded;
            _mod.Unloaded += OnModUnloaded;

            UpdateTitle();

            Shell.StatusBar.AddItem("Ready", new GridLength(1, GridUnitType.Star));
            _output.AppendLine("Started up");
        }

        void OnModLoaded(object sender, EventArgs e)
        {
            UpdateTitle();
        }

        void UpdateTitle()
        {
            var version = ZeldaVersion.Version;
            var title = "Zelda Mod Editor {0}".F(version);
            if (_mod.IsLoaded)
                title = _mod.Name + " - " + title;

            MainWindow.Title = title;
        }

        void OnModUnloaded(object sender, EventArgs e)
        {
            UpdateTitle();
        }
    }
}
