using System.Collections.Generic;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    class ModResources
    {
        readonly Game.ModResources _resources = new Game.ModResources();

        static readonly Dictionary<ResourceType, string> _resourceTypeFriendlyNames = new Dictionary<ResourceType, string>()
        {
            { ResourceType.Map,      "Map"                  },
            { ResourceType.TileSet,  "Tileset"              },
            { ResourceType.Sprite,   "Sprite"               },
            { ResourceType.Music,    "Music"                },
            { ResourceType.Sound,    "Sound"                },
            { ResourceType.Item,     "Item"                 },
            { ResourceType.Enemy,    "Enemy"                },
            { ResourceType.Entity,   "Custom entity"        },
            { ResourceType.Language, "Language"             },
            { ResourceType.Font,     "Font"                 },
        };

        static readonly Dictionary<ResourceType, string> _resourceTypeDirectoryFriendlyNames = new Dictionary<ResourceType, string>()
        {
            { ResourceType.Map,      "Map folder"           },
            { ResourceType.TileSet,  "Tileset folder"       },
            { ResourceType.Sprite,   "Sprite folder"        },
            { ResourceType.Music,    "Music folder"         },
            { ResourceType.Sound,    "Sound folder"         },
            { ResourceType.Item,     "Item folder"          },
            { ResourceType.Enemy,    "Enemy folder"         },
            { ResourceType.Entity,   "Custom entity folder" },
            { ResourceType.Language, "Language folder"      },
            { ResourceType.Font,     "Font folder"          },
        };

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

        public string GetDirectoryFriendlyName(ResourceType resourceType)
        {
            return _resourceTypeDirectoryFriendlyNames[resourceType];
        }

        public string GetTypeName(ResourceType resourceType)
        {
            return Game.ModResources.GetResourceTypeName(resourceType);
        }

        public string GetFriendlyName(ResourceType resourceType)
        {
            return _resourceTypeFriendlyNames[resourceType];
        }
    }
}
