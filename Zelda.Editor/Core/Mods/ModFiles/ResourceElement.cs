using System;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods.ModFiles
{
    class ResourceElement : ModFileBase
    {
        public override string Name { get { return System.IO.Path.GetFileNameWithoutExtension(Path); } }
        public override Uri Icon { get { return new Uri("/Resources/Icons/icon_resource_{0}.png".F(ResourceType.ToString().ToLower()), UriKind.Relative); } }
        public ResourceType ResourceType { get; private set; }

        public ResourceElement(ResourceType resourceType, string path, IModFile parent)
            : base(ModFileType.ResourceElement, path, parent)
        {
            ResourceType = resourceType;
        }
    }
}
