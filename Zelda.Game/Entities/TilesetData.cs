using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public enum TileScrolling
    {
        None,
        Parallax,
        Self
    }

    public class TilePatternData : IXmlDeserialized
    {
        [XmlAttribute]
        public string Id { get; set; }
        public Ground Ground { get; set; } = Ground.Traversable;
        public Layer DefaultLayer { get; set; } = Layer.Low;

        [DefaultValue(TileScrolling.None)]
        public TileScrolling Scrolling { get; set; } = TileScrolling.None;

        [XmlElement("X")]
        public int[] X { get; set; }

        [XmlElement("Y")]
        public int[] Y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        [XmlIgnore]
        public Rectangle[] Frames { get; set; }

        public TilePatternData()
        {
            SetFrame(new Rectangle(0, 0, 16, 16));
        }

        public void OnDeserialized()
        {
            int numX = X.Length;
            int numY = Y.Length;
            if (numX != 1 && numX != 3 && numX != 4)
                throw new InvalidDataException("Invalid number of frames for x");
            if (numY != 1 && numY != 3 && numY != 4)
                throw new InvalidDataException("Invalid number of frames for y");
            if (numX != numY)
                throw new InvalidDataException("The length of x and y must match");

            Frames = new Rectangle[numX];
            for (int i = 0; i < numX; ++i)
                Frames[i] = new Rectangle(X[i], Y[i], Width, Height);
        }

        public void SetFrame(Rectangle frame)
        {
            Frames = new Rectangle[] { frame };
        }
    }

    [XmlRoot("Tileset")]
    public class TilesetData : IXmlDeserialized
    {
        public Color BackgroundColor { get; set; } = Color.White;

        [XmlElement("TilePattern")]
        public TilePatternData[] PatternDatas { get; set; }

        [XmlIgnore]
        public IReadOnlyDictionary<string, TilePatternData> Patterns { get; private set; }

        public void OnDeserialized()
        {
            PatternDatas.Do(p => p.OnDeserialized());

            Patterns = PatternDatas.ToDictionary(p => p.Id, p => p);
        }
    }
}
