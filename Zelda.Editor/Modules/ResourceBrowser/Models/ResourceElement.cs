using System;
using Zelda.Editor.Core;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class ResourceElement : ModFileBase
    {
        string _description;

        public override ModFileType FileType { get { return ModFileType.ResourceElement; } }
        public override string Name { get { return System.IO.Path.GetFileNameWithoutExtension(Path); } }
        public override Uri Icon { get { return "/Resources/Icons/icon_resource_{0}.png".F(ResourceType.ToString().ToLower()).ToIconUri(); } }
        public ResourceType ResourceType { get; private set; }
        public override string Description { get { return _description; } }

        public ResourceElement(ResourceType resourceType, string description)
        {
            ResourceType = resourceType;
            _description = description;
        }

        public void SetDescription(string description)
        {
            this.SetProperty(ref _description, description, "Description");
        }
    }
}
