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

            [XmlArrayItem("Sprite")]
            public Resource[] Sprites { get; set; }

            [XmlArrayItem("Item")]
            public Resource[] Items { get; set; }
            
            [XmlArrayItem("Language")]
            public Resource[] Languages { get; set; }

            [XmlArrayItem("Font")]
            public Resource[] Fonts { get; set; }

            // TODO XmlChoiceIdentifierAttribute 사용
        }

        public class ResourceMap : Dictionary<string, string>
        {
        }

        string[] _assemblies;
        public IEnumerable<string> Assemblies
        {
            get { return _assemblies; }
        }
   
        readonly Dictionary<ResourceType, ResourceMap> _resourceMaps;

        public ModResources()
        {
            _resourceMaps = Enum.GetValues(typeof(ResourceType))
                .Cast<ResourceType>()
                .ToDictionary(t => t, t => new ResourceMap());
        }

        protected override bool ImportFromStream(byte[] buffer)
        {
            try
            {
                ProjectDB db = buffer.XmlDeserialize<ProjectDB>();

                _assemblies = db.Assemblies.ToArray();

                FillResourceMap(ResourceType.Map, db.Maps);
                FillResourceMap(ResourceType.Sprite, db.Sprites);
                FillResourceMap(ResourceType.Item, db.Items);
                FillResourceMap(ResourceType.Language, db.Languages);
                FillResourceMap(ResourceType.Font, db.Fonts);
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to load mod resource list 'project_db.xml': {0}".F(ex.Message));
                return false;
            }
            return true;
        }

        private void FillResourceMap(ResourceType resourceType, ProjectDB.Resource[] resources)
        {
            if (resources == null)
                return;

            foreach (ProjectDB.Resource resource in resources)
                _resourceMaps[resourceType].Add(resource.Id, resource.Description);
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
    }
}
