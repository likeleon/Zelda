using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Zelda.Game.LowLevel;
using Zelda.Game.Entities;
using System.ComponentModel;

namespace Zelda.Game
{
    public class EntityIndex
    {
        public Layer Layer { get; set; } = Layer.Low;
        public int Index { get; set; } = -1;
        public bool IsValid => Index != -1;

        public EntityIndex()
        {
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

    public class MapProperties
    {
        [DefaultValue(0)]
        public int X { get; set; } = 0;

        [DefaultValue(0)]
        public int Y { get; set; } = 0;

        public int Width { get; set; }
        public int Height { get; set; }

        [DefaultValue(null)]
        public string World { get; set; }

        [DefaultValue(MapData.NoFloor)]
        public int Floor { get; set; } = MapData.NoFloor;

        public string Tileset { get; set; }

        [DefaultValue(LowLevel.Music.None)]
        public string Music { get; set; } = LowLevel.Music.None;
    }

    public class MapData : IXmlDeserialized, IPrepareXmlSerialize
    {
        public const int NoFloor = 9999;

        public MapProperties Properties { get; set; }

        [XmlIgnore] public Point Location { get; private set; }
        [XmlIgnore] public Size Size { get; private set; }

        public string World { get; set; }
        [XmlIgnore] public bool HasWorld => World != null;

        public int Floor { get; set; }
        [XmlIgnore] public bool HasFloor => Floor != NoFloor;

        public string TilesetId { get; set; }

        public string MusicId { get; set; }
        [XmlIgnore] public bool HasMusic => MusicId != Music.None;

        [XmlChoiceIdentifier("EntityTypes")]
        [XmlElement("Tile", typeof(TileData))]
        [XmlElement("Destination", typeof(DestinationData))]
        [XmlElement("Destructible", typeof(DestructibleData))]
        [XmlElement("Chest", typeof(ChestData))]
        [XmlElement("Npc", typeof(NpcData))]
        [XmlElement("Block", typeof(BlockData))]
        [XmlElement("DynamicTile", typeof(DynamicTileData))]
        public EntityData[] EntityDatas { get; set; }

        [XmlIgnore]
        public EntityType[] EntityTypes;

        readonly List<EntityData>[] _entities = new List<EntityData>[(int)Layer.NumLayer];
        readonly Dictionary<string, EntityIndex> _namedEntities = new Dictionary<string, EntityIndex>();

        public MapData()
        {
            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
                _entities[layer] = new List<EntityData>();
        }

        public void OnDeserialized()
        {
            Location = new Point(Properties.X, Properties.Y);
            Size = new Size(Properties.Width, Properties.Height);
            TilesetId = Properties.Tileset;
            World = Properties.World;
            Floor = Properties.Floor;
            MusicId = Properties.Music;

            foreach (var e in EntityDatas)
            {
                e.OnDeserialized();
                AddEntity(e);
            }
        }

        public void OnPrepareSerialize()
        {
            Properties = new MapProperties()
            {
                X = Location.X,
                Y = Location.Y,
                Width = Size.Width,
                Height = Size.Height,
                Tileset = TilesetId,
                World = World,
                Floor = Floor,
                Music = MusicId
            };

            throw new NotImplementedException("PrepareSerialize on EntityDatas");
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
            if (!EntityExists(index))
                throw new ArgumentOutOfRangeException(nameof(index), "Entity index out of range");

            return _entities[(int)index.Layer][index.Index];
        }
    }
}
