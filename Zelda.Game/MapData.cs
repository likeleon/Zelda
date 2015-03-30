using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game
{
    class EntityIndex
    {
        public Layer Layer { get; set; }
        
        public int Index { get; set; }

        public bool IsValid
        {
            get { return Index != -1; }
        }

        public EntityIndex()
        {
            Layer = Layer.Low;
            Index = -1;
        }

        public EntityIndex(Layer layer, int index)
        {
            Layer = layer;
            Index = index;
        }

        public static bool operator ==(EntityIndex index1, EntityIndex index2)
        {
            return (index1.Layer == index2.Layer) &&
                   (index2.Index == index2.Index);
        }

        public static bool operator !=(EntityIndex index1, EntityIndex index2)
        {
            return (index1.Layer != index2.Layer) ||
                   (index2.Index != index2.Index);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EntityIndex))
                return false;

            return (this == (EntityIndex)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class MapData : XmlData
    {
        // 맵의 크기, 픽셀 단위
        public Size Size { get; private set; }

        // 맵의 월드
        public string World { get; private set; }

        // 월드 상에서의 이 맵의 좌측 최상단 좌표
        public Point Location { get; private set; }

        public int Floor { get; private set; }

        public string TilesetId { get; private set; }

        public static readonly int NoFloor = 9999;

        readonly List<EntityData>[] _entities = new List<EntityData>[Enum.GetValues(typeof(Layer)).Length];
        readonly Dictionary<string, EntityIndex> _namedEntities = new Dictionary<string, EntityIndex>();

        public MapData()
        {
            World = String.Empty;
            foreach (Layer layer in Enum.GetValues(typeof(Layer)))
                _entities[(int)layer] = new List<EntityData>();
        }

        protected override bool ImportFromStream(Stream stream)
        {
            try
            {
                MapXmlData data = stream.XmlDeserialize<MapXmlData>();
                int x = data.Properties.X.OptField(0);
                int y = data.Properties.Y.OptField(0);
                int width = data.Properties.Width.CheckField("Width");
                int height = data.Properties.Height.CheckField("Height");
                string world = data.Properties.World.OptField(String.Empty);
                int floor = data.Properties.Floor.OptField(MapData.NoFloor);
                string tilesetId = data.Properties.Tileset.CheckField("Tileset");

                Location = new Point(x, y);
                Size = new Size(width, height);
                World = world;
                Floor = floor;
                TilesetId = tilesetId;

                foreach (var entity in data.Entities)
                    AddEntity(EntityData.CheckEntityData(entity));
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to load map: {0}".F(ex.Message));
                return false;
            }
            return true;
        }

        EntityIndex AddEntity(EntityData entity)
        {
            Layer layer = entity.Layer;
            EntityIndex index = new EntityIndex(layer, _entities[(int)layer].Count);

            if (!entity.Type.CanBeStoredInMapFile())
                return new EntityIndex();

            if (entity.HasName)
            {
                if (EntityExists(entity.Name))
                    return new EntityIndex();   // 이름 중복

                _namedEntities.Add(entity.Name, index);
            }

            _entities[(int)layer].Add(entity);
            
            return index;
        }

        public bool EntityExists(string name)
        {
            return _namedEntities.ContainsKey(name);
        }
        
        public bool EntityExists(EntityIndex index)
        {
            return (index.Index >= 0) && (index.Index < GetNumEntities(index.Layer));
        }

        public int GetNumEntities(Layer layer)
        {
            return _entities[(int)layer].Count;
        }

        public EntityData GetEntity(EntityIndex index)
        {
            Debug.CheckAssertion(EntityExists(index), "Entity index out of range");
            return _entities[(int)index.Layer][index.Index];
        }
    }

    [XmlRoot("MapData")]
    public class MapXmlData
    {
        public class PropertiesData
        {
            public int? X { get; set; }
            public int? Y { get; set; }
            public int? Width { get; set; }
            public int? Height { get; set; }
            public string World { get; set; }
            public int? Floor { get; set; }
            public string Tileset { get; set; }
        }

        public PropertiesData Properties { get; set; }

        [XmlChoiceIdentifier("EntityTypes")]
        [XmlElement("Tile", typeof(TileXmlData))]
        [XmlElement("Destination", typeof(DestinationXmlData))]
        public EntityXmlData[] Entities { get; set; }

        [XmlIgnore]
        public EntityType[] EntityTypes;
    }
}
