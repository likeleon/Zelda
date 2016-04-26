using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.LowLevel;
using Zelda.Game.Entities;

namespace Zelda.Game
{
    public class EntityIndex
    {
        public Layer Layer { get; set; }
        public int Index { get; set; }
        public bool IsValid { get { return Index != -1; } }

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

    public class MapData : XmlData
    {
        public Size Size { get; set; }
        public bool HasWorld { get { return !World.IsNullOrEmpty(); } }
        public string World { get; private set; } = "";
        public Point Location { get; private set; }
        public bool HasFloor { get { return Floor != NoFloor; } }
        public int Floor { get; private set; }
        public string TilesetId { get; set; }
        public bool HasMusic { get { return MusicId != Music.None; } }
        public string MusicId { get; private set; }

        public static readonly int NoFloor = 9999;

        readonly List<EntityData>[] _entities = new List<EntityData>[(int)Layer.NumLayer];
        readonly Dictionary<string, EntityIndex> _namedEntities = new Dictionary<string, EntityIndex>();

        public MapData()
        {
            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
                _entities[layer] = new List<EntityData>();
        }

        protected override bool OnImportFromBuffer(byte[] buffer)
        {
            try
            {
                var data = buffer.XmlDeserialize<MapXmlData>();
                Location = new Point(data.Properties.X.OptField(0), data.Properties.Y.OptField(0));
                Size = new Size(data.Properties.Width.CheckField("Width"), data.Properties.Height.CheckField("Height"));
                World = data.Properties.World.OptField(String.Empty);
                Floor = data.Properties.Floor.OptField(MapData.NoFloor);
                TilesetId = data.Properties.Tileset.CheckField("Tileset");
                MusicId = data.Properties.Music.OptField(Music.None);

                foreach (var entity in data.Entities)
                    AddEntity(EntityData.Create(entity));
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
            var layer = entity.Layer;
            var index = new EntityIndex(layer, _entities[(int)layer].Count);

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

        protected override bool OnExportToStream(Stream stream)
        {
            try
            {
                var data = new MapXmlData();
                data.Properties = new MapXmlData.PropertiesData()
                {
                    X = Location.X,
                    Y = Location.Y,
                    Width = Size.Width,
                    Height = Size.Height,
                    Tileset = TilesetId
                };
                if (HasWorld)
                    data.Properties.World = World;
                if (HasFloor)
                    data.Properties.Floor = Floor;
                if (HasMusic)
                    data.Properties.Music = MusicId;

                for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
                {
                    foreach (var entity in _entities[layer])
                    {
                        var success = entity.ExportToStream(stream);
                        Debug.CheckAssertion(success, "Entity export failed");
                    }
                }
                data.XmlSerialize(stream);
                return true;
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to export map: {0}".F(ex));
                return false;
            }
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
            public string Music { get; set; }
        }

        public PropertiesData Properties { get; set; }

        [XmlChoiceIdentifier("EntityTypes")]
        [XmlElement("Tile", typeof(TileXmlData))]
        [XmlElement("Destination", typeof(DestinationXmlData))]
        [XmlElement("Destructible", typeof(DestructibleXmlData))]
        [XmlElement("Chest", typeof(ChestXmlData))]
        [XmlElement("Npc", typeof(NpcXmlData))]
        [XmlElement("Block", typeof(BlockXmlData))]
        [XmlElement("DynamicTile", typeof(DynamicTileXmlData))]
        public EntityXmlData[] Entities { get; set; }

        [XmlIgnore]
        public EntityType[] EntityTypes;
    }
}
