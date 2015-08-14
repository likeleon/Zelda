using System;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    class ModResources
    {
        readonly Game.ModResources _resources = new Game.ModResources();

        public static ModResources Load(string rootPath)
        {
            var modResources = new ModResources();
            modResources._resources.ImportFromFile(rootPath);
            return modResources;
        }

        public bool Exists(ResourceType type, string id)
        {
            return _resources.Exists(type, id);
        }

        public string GetDescription(ResourceType type, string id)
        {
            return _resources.GetDescription(type, id);
        }
    }
}
