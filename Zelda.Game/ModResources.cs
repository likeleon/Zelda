using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Zelda.Game
{
    public class ModResources : XmlData
    {
        public class ProjectDB
        {
            public class Resource
            {
                [XmlAttribute]
                public string Id { get; set; }

                [XmlAttribute]
                public string Description { get; set; }
            }

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

            // TODO XmlChoiceIdentifierAttribute 사용
        }

        public class ResourceMap : Dictionary<string, string>
        {
        }


        public IEnumerable<string> Assemblies
        {
            get { return _assemblies; }
        }
   
        readonly static Dictionary<ResourceType, string> _resourceTypeNames;

        readonly Dictionary<ResourceType, ResourceMap> _resourceMaps;

        string[] _assemblies;

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

        protected override bool ImportFromBuffer(byte[] buffer)
        {
            try
            {
                var db = buffer.XmlDeserialize<ProjectDB>();

                _assemblies = db.Assemblies.ToArray();

                FillResourceMap(ResourceType.Map, db.Maps);
                FillResourceMap(ResourceType.Tileset, db.Tilesets);
                FillResourceMap(ResourceType.Sprite, db.Sprites);
                FillResourceMap(ResourceType.Music, db.Musics);
                FillResourceMap(ResourceType.Sound, db.Sounds);
                FillResourceMap(ResourceType.Item, db.Items);
                FillResourceMap(ResourceType.Enemy, db.Enemies);
                FillResourceMap(ResourceType.Entity, db.Entities);
                FillResourceMap(ResourceType.Language, db.Languages);
                FillResourceMap(ResourceType.Font, db.Fonts);
                return true;
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to load mod resource list 'project_db.xml': {0}".F(ex));
                return false;
            }
        }

        void FillResourceMap(ResourceType resourceType, ProjectDB.Resource[] resources)
        {
            if (resources == null)
                return;

            foreach (var resource in resources)
                _resourceMaps[resourceType].Add(resource.Id, resource.Description);
        }

        protected override bool ExportToStream(Stream stream)
        {
            try
            {
                var db = new ProjectDB();
                db.Assemblies = _assemblies.ToArray();

                db.Maps = ExportResourceMap(ResourceType.Map);
                db.Tilesets = ExportResourceMap(ResourceType.Tileset);
                db.Sprites = ExportResourceMap(ResourceType.Sprite);
                db.Musics = ExportResourceMap(ResourceType.Music);
                db.Sounds = ExportResourceMap(ResourceType.Sound);
                db.Items = ExportResourceMap(ResourceType.Item);
                db.Enemies = ExportResourceMap(ResourceType.Enemy);
                db.Entities = ExportResourceMap(ResourceType.Entity);
                db.Languages = ExportResourceMap(ResourceType.Language);
                db.Fonts = ExportResourceMap(ResourceType.Font);

                db.XmlSerialize(stream);
                return true;
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to save mod resource list 'project_db.xml': {0}".F(ex));
                return false;
            }
        }

        ProjectDB.Resource[] ExportResourceMap(ResourceType resourceType)
        {
            return _resourceMaps[resourceType]
            .Select(kvp => new ProjectDB.Resource() { Id = kvp.Key, Description = kvp.Value })
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

        public void Clear()
        {
            foreach (var resourceMap in _resourceMaps)
            {
                resourceMap.Value.Clear();
            }
        }

        public bool Exists(ResourceType resourceType, string id)
        {
            ResourceMap resource = GetElements(resourceType);
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
