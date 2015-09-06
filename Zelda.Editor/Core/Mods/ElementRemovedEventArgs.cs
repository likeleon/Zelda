using System;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    class ElementRemovedEventArgs : EventArgs
    {
        public ResourceType ResourceType { get; private set; }
        public string Id { get; private set; }

        public ElementRemovedEventArgs(ResourceType resourceType, string id)
        {
            ResourceType = resourceType;
            Id = id;
        }
    }
}
