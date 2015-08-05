using System;

namespace Zelda.Editor.Core.Mods.ModFiles
{
    class UnknownFile : ModFileBase
    {
        public override Uri Icon { get { return new Uri("/Resources/Icons/icon_file_unknown.png", UriKind.Relative); } }

        public UnknownFile(string path, IModFile parent)
            : base(ModFileType.Unknown, path, parent)
        {
        }
    }
}
