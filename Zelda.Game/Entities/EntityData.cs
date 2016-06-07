using System;
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

        internal abstract void CreateEntity(Map map);

        protected static Size EntityCreationCheckSize(int width, int height)
        {
            if (width < 0 || width % 8 != 0)
                throw new Exception("Invalid width {0}: should be a positive multiple of 8".F(width));

            if (height < 0 || height % 8 != 0)
                throw new Exception("Invalid height {0}: should be a positive multiple of 8".F(height));

            return new Size(width, height);
        }
    }
}
