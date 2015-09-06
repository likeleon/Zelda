using System;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    class ElementDescriptionChangedEventArgs : EventArgs
    {
        public ResourceType ResourceType { get; private set; }
        public string Id { get; private set; }
        public string Description { get; private set; }

        public ElementDescriptionChangedEventArgs(ResourceType resourceType, string id, string description)
        {
            ResourceType = resourceType;
            Id = id;
            Description = description;
        }
    }
}
