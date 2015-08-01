using System.ComponentModel.Composition;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.ModEditor.Services;

namespace Zelda.Editor.Modules.ResourceBrowser.ViewModels
{
    [Export(typeof(IResourceBrowser))]
    class ResourceBrowserViewModel : Tool, IResourceBrowser
    {
        readonly IMod _mod;

        public override PaneLocation PreferredLocation { get { return PaneLocation.Left; } }
        public IMod Mod { get { return _mod; } }

        [ImportingConstructor]
        public ResourceBrowserViewModel(IMod mod)
        {
            _mod = mod;
        }
    }
}
