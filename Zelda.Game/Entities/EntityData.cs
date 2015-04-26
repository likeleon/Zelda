using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    class EntityData
    {
        readonly EntityType _type;
        public EntityType Type
        {
            get { return _type; }
        }

        public string Name { get; set; }
        
        public bool HasName
        {
            get { return !String.IsNullOrEmpty(Name); }
        }

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
            else
                throw new Exception("Unknown entity type");
        }

        public EntityData(EntityType type, EntityXmlData xmlData)
        {
            _type = type;
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
