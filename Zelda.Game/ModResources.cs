using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Zelda.Game
{
    [XmlRoot("ProjectDB")]
    public class ModResources : IXmlDeserialized, IXmlSerializing
    {
        public class Resource
        {
            [XmlAttribute]
            public string Id { get; set; }

            [XmlAttribute]
            public string Description { get; set; }
        }

        public class ResourceMap : Dictionary<string, string>
        {
        }

        // TODO XmlChoiceIdentifierAttribute 사용
        [XmlArrayItem("Assembly")]
        public string[] Assemblies { get; set; }

        [XmlArrayItem("Map")]
        public Resource[] Maps { get; set; }

        [XmlArrayItem("Tileset")]
        public Resource[] Tilesets { get; set; }

        [XmlArrayItem("Sprite")]
        public Resource[] Sprites { get; set; }

        [XmlArrayItem("Music")]
        public Resource[] Musics { get; set; }

        [XmlArrayItem("Sound")]
        public Resource[] Sounds { get; set; }

        [XmlArrayItem("Item")]
        public Resource[] Items { get; set; }

        [XmlArrayItem("Enemy")]
        public Resource[] Enemies { get; set; }

        [XmlArrayItem("Entity")]
        public Resource[] Entities { get; set; }

        [XmlArrayItem("Language")]
        public Resource[] Languages { get; set; }

        [XmlArrayItem("Font")]
        public Resource[] Fonts { get; set; }

        static readonly IReadOnlyDictionary<ResourceType, string> _resourceTypeNames;

        readonly Dictionary<ResourceType, ResourceMap> _resourceMaps = new Dictionary<ResourceType, ResourceMap>();

        static ModResources()
        {
            _resourceTypeNames = Enum.GetValues(typeof(ResourceType))
                .Cast<ResourceType>()
                .ToDictionary(t => t, t => t.ToString().ToLower());
        }

        public ModResources()
        {
            _resourceMaps = Enum.GetValues(typeof(ResourceType))
                .Cast<ResourceType>()
                .ToDictionary(t => t, t => new ResourceMap());
        }

        public void OnDeserialized()
        {
            FillResourceMap(ResourceType.Map, Maps);
            FillResourceMap(ResourceType.Tileset, Tilesets);
            FillResourceMap(ResourceType.Sprite, Sprites);
            FillResourceMap(ResourceType.Music, Musics);
            FillResourceMap(ResourceType.Sound, Sounds);
            FillResourceMap(ResourceType.Item, Items);
            FillResourceMap(ResourceType.Enemy, Enemies);
            FillResourceMap(ResourceType.Entity, Entities);
            FillResourceMap(ResourceType.Language, Languages);
            FillResourceMap(ResourceType.Font, Fonts);
        }

        void FillResourceMap(ResourceType resourceType, Resource[] resources)
        {
            resources?.Do(r => _resourceMaps[resourceType].Add(r.Id, r.Description));
        }

        public void OnSerializing()
        {
            Maps = ExportResourceMap(ResourceType.Map);
            Tilesets = ExportResourceMap(ResourceType.Tileset);
            Sprites = ExportResourceMap(ResourceType.Sprite);
            Musics = ExportResourceMap(ResourceType.Music);
            Sounds = ExportResourceMap(ResourceType.Sound);
            Items = ExportResourceMap(ResourceType.Item);
            Enemies = ExportResourceMap(ResourceType.Enemy);
            Entities = ExportResourceMap(ResourceType.Entity);
            Languages = ExportResourceMap(ResourceType.Language);
            Fonts = ExportResourceMap(ResourceType.Font);
        }

        Resource[] ExportResourceMap(ResourceType resourceType)
        {
            return _resourceMaps[resourceType]
            .Select(kvp => new Resource() { Id = kvp.Key, Description = kvp.Value })
            .ToArray();
        }

        public bool Add(ResourceType resourceType, string id, string description)
        {
            var resource = GetElements(resourceType);
            if (resource.ContainsKey(id))
                return false;

            resource.Add(id, description);
            return true;
        }

        public bool Remove(ResourceType resourceType, string id)
        {
            var resource = GetElements(resourceType);
            return resource.Remove(id);
        }

        public bool Rename(ResourceType resourceType, string oldId, string newId)
        {
            if (!Exists(resourceType, oldId))
                return false;
            if (Exists(resourceType, oldId))
                return false;

            var description = GetDescription(resourceType, oldId);
            Remove(resourceType, oldId);
            Add(resourceType, newId, description);
            return true;
        }

        public bool SetDescription(ResourceType resourceType, string id, string description)
        {
            if (!Exists(resourceType, id))
                return false;

            var resources = GetElements(resourceType);
            resources[id] = description;
            return true;
        }

        public void Clear()
        {
            foreach (var resourceMap in _resourceMaps)
            {
                resourceMap.Value.Clear();
            }
        }

        public bool Exists(ResourceType resourceType, string id)
        {
            var resource = GetElements(resourceType);
            return resource.ContainsKey(id);
        }

        public ResourceMap GetElements(ResourceType resourceType)
        {
            return _resourceMaps[resourceType];
        }

        public string GetDescription(ResourceType resourceType, string id)
        {
            var resource = GetElements(resourceType);
            if (!resource.ContainsKey(id))
                return string.Empty;
            return resource[id];
        }

        public static string GetResourceTypeName(ResourceType resourceType)
        {
            return _resourceTypeNames[resourceType];
        }
    }
}
