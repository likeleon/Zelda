using System;

namespace Zelda.Editor.Core.Mods.ModFiles
{
    class NormalDirectory : ModFileBase
    {
        public override string Name { get { return System.IO.Path.GetFileName(Path); } }
        public override Uri Icon { get { return new Uri("/Resources/Icons/icon_folder_open.png", UriKind.Relative); } }

        public NormalDirectory(string path, IModFile parent)
            : base(ModFileType.NormalDirectory, path, parent)
        {
        }
    }
}
