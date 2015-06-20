using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    public class EntityData
    {
        public EntityType Type { get; private set;  }
        public string Name { get; set; }
        public bool HasName { get { return !String.IsNullOrEmpty(Name); } }
        public Layer Layer { get; set; }
        public Point XY { get; set; }

        public static EntityData Create(EntityXmlData xmlData)
        {
            if (xmlData is TileXmlData)
                return new TileData(xmlData as TileXmlData);
            else if (xmlData is DestinationXmlData)
                return new DestinationData(xmlData as DestinationXmlData);
            else if (xmlData is DestructibleXmlData)
                return new DestructibleData(xmlData as DestructibleXmlData);
            else if (xmlData is ChestXmlData)
                return new ChestData(xmlData as ChestXmlData);
            else if (xmlData is NpcXmlData)
                return new NpcData(xmlData as NpcXmlData);
            else if (xmlData is BlockXmlData)
                return new BlockData(xmlData as BlockXmlData);
            else if (xmlData is DynamicTileXmlData)
                return new DynamicTileData(xmlData as DynamicTileXmlData);
            else
                throw new Exception("Unknown entity type");
        }
        
        public EntityData(EntityType type)
        {
            Type = type;
        }

        public EntityData(EntityType type, EntityXmlData xmlData)
        {
            Type = type;
            Name = xmlData.Name.OptField("");
            Layer = xmlData.Layer.CheckField<Layer>("Layer");
            XY = new Point(xmlData.X.CheckField("X"), xmlData.Y.CheckField("Y"));
        }
    }

    public abstract class EntityXmlData
    {
        public string Name { get; set; }
        public Layer? Layer { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
    }
}
