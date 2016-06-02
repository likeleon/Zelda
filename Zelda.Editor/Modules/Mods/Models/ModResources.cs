using System;
using System.Collections.Generic;
using Zelda.Game;

namespace Zelda.Editor.Modules.Mods.Models
{
    class ModResources
    {
        public event EventHandler<ElementAddedEventArgs> ElementAdded;
        public event EventHandler<ElementRemovedEventArgs> ElementRemoved;
        public event EventHandler<ElementRenamedEventArgs> ElementRenamed;
        public event EventHandler<ElementDescriptionChangedEventArgs> ElementDescriptionChanged;

        readonly Game.ModResources _resources = new Game.ModResources();
        readonly IMod _mod;

        static readonly Dictionary<ResourceType, string> _resourceTypeFriendlyNames = new Dictionary<ResourceType, string>()
        {
            { ResourceType.Map,      "Map"                  },
            { ResourceType.Tileset,  "Tileset"              },
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
            { ResourceType.Tileset,  "Tileset folder"       },
            { ResourceType.Sprite,   "Sprite folder"        },
            { ResourceType.Music,    "Music folder"         },
            { ResourceType.Sound,    "Sound folder"         },
            { ResourceType.Item,     "Item folder"          },
            { ResourceType.Enemy,    "Enemy folder"         },
            { ResourceType.Entity,   "Custom entity folder" },
            { ResourceType.Language, "Language folder"      },
            { ResourceType.Font,     "Font folder"          },
        };

        static readonly Dictionary<ResourceType, string> _resourceTypeCreateFriendlyNames = new Dictionary<ResourceType, string>()
        {
            { ResourceType.Map,      "New map..."                 },
            { ResourceType.Tileset,  "New tileset..."             },
            { ResourceType.Sprite,   "New sprite..."              },
            { ResourceType.Music,    "New music..."               },
            { ResourceType.Sound,    "New sound..."               },
            { ResourceType.Item,     "New item..."                },
            { ResourceType.Enemy,    "New enemy breed..."         },
            { ResourceType.Entity,   "New custom entity model..." },
            { ResourceType.Language, "New language..."            },
            { ResourceType.Font,     "New font..."                },
        };

        public ModResources(IMod mod, string rootPath)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");

            _mod = mod;
            _resources = XmlLoader.Load<Game.ModResources>(rootPath);
        }

        public bool Add(ResourceType resourceType, string id, string description)
        {
            if (!_resources.Add(resourceType, id, description))
                return false;

            if (ElementAdded != null)
                ElementAdded(this, new ElementAddedEventArgs(resourceType, id, description));

            return true;
        }

        public bool Remove(ResourceType resourceType, string id)
        {
            if (!_resources.Remove(resourceType, id))
                return false;

            if (ElementRemoved != null)
                ElementRemoved(this, new ElementRemovedEventArgs(resourceType, id));

            return true;
        }

        public bool Rename(ResourceType type, string oldId, string newId)
        {
            if (!_resources.Rename(type, oldId, newId))
                return false;

            if (ElementRenamed != null)
                ElementRenamed(this, new ElementRenamedEventArgs(type, oldId, newId));
            return true;
        }

        public bool SetDescription(ResourceType type, string id, string description)
        {
            if (!_resources.SetDescription(type, id, description))
                return false;

            if (ElementDescriptionChanged != null)
                ElementDescriptionChanged(this, new ElementDescriptionChangedEventArgs(type, id, description));
            return true;
        }

        public void Save()
        {
            var fileName = _mod.GetResourceListPath();
            if (!_resources.ExportToFile(fileName))
                throw new Exception("Cannot write file '{0}'".F(fileName));
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

        public string GetCreateFriendlyName(ResourceType resourceType)
        {
            return _resourceTypeCreateFriendlyNames[resourceType];
        }

        public IEnumerable<string> GetElements(ResourceType resourceType)
        {
            return _resources.GetElements(resourceType).Keys;
        }
    }
}
