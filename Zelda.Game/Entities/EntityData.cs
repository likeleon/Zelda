using System.ComponentModel;
using System.Xml.Serialization;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public abstract class EntityData : IXmlDeserialized, IPrepareXmlSerialize
    {
        [XmlIgnore]
        public abstract EntityType Type { get; }

        [DefaultValue(null)]
        public string Name { get; set; }
        public Layer Layer { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        [XmlIgnore]
        public bool HasName => Name != null;
        public Point XY { get; set; }

        public virtual void OnDeserialized()
        {
            XY = new Point(X, Y);
        }

        public virtual void OnPrepareSerialize()
        {
            X = XY.X;
            Y = XY.Y;
        }
    }
}
