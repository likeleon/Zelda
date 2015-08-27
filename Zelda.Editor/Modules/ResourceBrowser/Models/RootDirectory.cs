using System;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class RootDirectory : ModFileBase
    {
        readonly string _modName;

        public override ModFileType FileType { get { return ModFileType.RootDirectory; } }
        public override string Name { get { return _modName; } }
        public override Uri Icon { get { return new Uri("/Resources/Icons/icon_solarus.png", UriKind.Relative); } }

        public RootDirectory(string modName)
        {
            if (modName.IsNullOrEmpty())
                throw new ArgumentNullException("modName");

            _modName = modName;
        }
    }
}
