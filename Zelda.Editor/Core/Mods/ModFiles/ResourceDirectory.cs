using System;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods.ModFiles
{
    class ResourceDirectory : ModFileBase
    {
        public override string Name { get { return System.IO.Path.GetFileName(Path); } }
        public override Uri Icon { get { return new Uri("/Resources/Icons/icon_folder_open_{0}.png".F(ResourceType.ToString().ToLower()), UriKind.Relative); } }
        public ResourceType ResourceType { get; private set; }

        public ResourceDirectory(ResourceType resourceType, string path, IModFile parent)
            : base(ModFileType.NormalDirectory, path, parent)
        {
            ResourceType = resourceType;
        }
    }
}
