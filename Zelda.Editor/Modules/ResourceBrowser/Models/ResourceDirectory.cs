using System;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class ResourceDirectory : ModFileBase
    {
        public override ModFileType FileType { get { return ModFileType.ResourceDirectory; } }
        public override string Name { get { return System.IO.Path.GetFileName(Path); } }
        public override Uri Icon { get { return new Uri("/Resources/Icons/icon_folder_open_{0}.png".F(ResourceType.ToString().ToLower()), UriKind.Relative); } }
        public ResourceType ResourceType { get; private set; }

        public ResourceDirectory(ResourceType resourceType)
        {
            ResourceType = resourceType;
        }
    }
}
