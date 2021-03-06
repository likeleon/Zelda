﻿using System;
using Zelda.Game;

namespace Zelda.Editor.Modules.Mods.Models
{
    class ElementAddedEventArgs : EventArgs
    {
        public ResourceType ResourceType { get; private set; }
        public string Id { get; private set; }
        public string Description { get; private set; }

        public ElementAddedEventArgs(ResourceType resourceType, string id, string description)
        {
            ResourceType = resourceType;
            Id = id;
            Description = description;
        }
    }
}
