using System;
using Zelda.Game;

namespace Zelda.Editor.Modules.Mods.Models
{
    class ElementRenamedEventArgs : EventArgs
    {
        public ResourceType ResourceType { get; private set; }
        public string OldId { get; private set; }
        public string NewId { get; private set; }

        public ElementRenamedEventArgs(ResourceType resourceType, string oldId, string newId)
        {
            ResourceType = resourceType;
            OldId = oldId;
            NewId = newId;
        }
    }
}
