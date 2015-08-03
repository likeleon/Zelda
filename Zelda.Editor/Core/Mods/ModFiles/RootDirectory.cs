using System;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods.ModFiles
{
    class RootDirectory : ModFileBase
    {
        readonly string _modName;

        public override string Name { get { return _modName; } }
        public override Uri Icon { get { return new Uri("/Resources/Icons/icon_solarus.png", UriKind.Relative); } }

        public RootDirectory(string modName, string path, IModFile parent)
            : base(ModFileType.RootDirectory, path, parent)
        {
            if (modName.IsNullOrEmpty())
                throw new ArgumentNullException("modName");

            _modName = modName;
        }
    }
}
