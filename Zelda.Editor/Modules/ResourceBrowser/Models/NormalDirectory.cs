using System;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class NormalDirectory : ModFileBase
    {
        public override ModFileType FileType { get { return ModFileType.NormalDirectory; } }
        public override Uri Icon { get { return "/Resources/Icons/icon_folder_open.png".ToIconUri(); } }

        protected override void OnPathChanged()
        {
            Name = System.IO.Path.GetFileName(Path);
        }
    }
}
