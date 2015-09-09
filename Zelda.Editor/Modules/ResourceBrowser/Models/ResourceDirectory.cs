using System;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class ResourceDirectory : ModFileBase
    {
        public override ModFileType FileType { get { return ModFileType.ResourceDirectory; } }
        public override Uri Icon { get { return "/Resources/Icons/icon_folder_open_{0}.png".F(ResourceType.ToString().ToLower()).ToIconUri(); } }
        public ResourceType ResourceType { get; private set; }

        public ResourceDirectory(ResourceType resourceType)
        {
            ResourceType = resourceType;
        }

        protected override void OnPathChanged()
        {
            Name = System.IO.Path.GetFileName(Path);
        }
    }
}
