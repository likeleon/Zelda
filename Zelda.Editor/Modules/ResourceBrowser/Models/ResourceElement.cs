using System;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class ResourceElement : ModFileBase
    {
        public override ModFileType FileType { get { return ModFileType.ResourceElement; } }
        public override Uri Icon { get { return "/Resources/Icons/icon_resource_{0}.png".F(ResourceType.ToString().ToLower()).ToIconUri(); } }
        public ResourceType ResourceType { get; private set; }

        public ResourceElement(ResourceType resourceType)
        {
            ResourceType = resourceType;
        }

        protected override void OnPathChanged()
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
        }
    }
}
