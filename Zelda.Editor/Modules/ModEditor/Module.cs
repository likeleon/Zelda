using System;
using System.ComponentModel.Composition;
using Zelda.Editor.Core;
using Zelda.Editor.Modules.ModEditor.Services;
using Zelda.Game;

namespace Zelda.Editor.Modules.ModEditor
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        readonly IMod _mod;

        [ImportingConstructor]
        public Module(IMod mod)
        {
            _mod = mod;
        }

        public override void Initialize()
        {
            _mod.Loaded += OnModLoaded;
            _mod.Unloaded += OnModUnloaded;

            UpdateTitle();
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
