using System;

namespace Zelda.Editor.Core.Mods.ModFiles
{
    class UnknownFile : ModFileBase
    {
        public override ModFileType FileType { get { return ModFileType.Unknown; } }
        public override Uri Icon { get { return new Uri("/Resources/Icons/icon_file_unknown.png", UriKind.Relative); } }
    }
}
