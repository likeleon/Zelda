using System;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods.ModFiles
{
    class ModRootDirectory : ModFileBase
    {
        readonly string _modName;

        public override string Name { get { return _modName; } }
        public override Uri Icon { get { return new Uri("/Resources/Icons/icon_solarus.png", UriKind.Relative); } }

        public ModRootDirectory(string modName, string path)
            : base(ModFileType.RootDirectory, path)
        {
            if (modName.IsNullOrEmpty())
                throw new ArgumentNullException("modName");

            _modName = modName;
        }
    }
}
