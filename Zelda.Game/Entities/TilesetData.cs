using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Zelda.Game.Entities
{
    public enum TileScrolling
    {
        None,
        Parallax,
        Self
    }

    class TilePatternData
    {
        public Ground Ground { get; set; }
        public Layer DefaultLayer { get; set; }
        public TileScrolling Scrolling { get; set; }
        public Rectangle[] Frames { get; set; }

        public TilePatternData()
            : this(new Rectangle(0, 0, 16, 16))
        {
        }

        public TilePatternData(Rectangle frame)
        {
            Ground = Entities.Ground.Traversable;
            DefaultLayer = Layer.Low;
            Scrolling = TileScrolling.None;
            Frames = new Rectangle[] { frame };
        }
    }

    class TilesetData : XmlData
    {
        [Description("타일셋의 배경색")]
        Color _backgroundColor = Color.White;
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        readonly Dictionary<string, TilePatternData> _patterns = new Dictionary<string, TilePatternData>();
        public IDictionary<string, TilePatternData> Patterns
        {
            get { return _patterns; }
        }

        protected override bool ImportFromStream(byte[] buffer)
        {
            try
            {
                TilesetXmlData data = buffer.XmlDeserialize<TilesetXmlData>();
                BackgroundColor = data.BackgroundColor.CheckColor("BackgroundColor");
                foreach (var pattern in data.TilePatterns)
                {
                    string id = pattern.Id.CheckField("Id");
                
                    TilePatternData patternData = new TilePatternData();
                    patternData.Ground = pattern.Ground.CheckField<Ground>("Ground");
                    patternData.DefaultLayer = pattern.DefaultLayer.CheckField<Layer>("DefaultLayer");
                    patternData.Scrolling = pattern.Scrolling.OptField<TileScrolling>("Scrolling", TileScrolling.None);
                    
                    int width = pattern.Width.CheckField("Width");
                    int height = pattern.Height.CheckField("Height");
                    int numX = pattern.X.Length;
                    int numY = pattern.Y.Length;
                    if (numX != 1 && numX != 3 && numX != 4)
                        throw new InvalidDataException("Invalid number of frames for x");
                    if (numY != 1 && numY != 3 && numY != 4)
                        throw new InvalidDataException("Invalid number of frames for y");
                    if (numX != numY)
                        throw new InvalidDataException("The length of x and y must match");

                    Rectangle[] frames = new Rectangle[numX];
                    for (int i = 0; i < pattern.X.Length; ++i)
                        frames[i] = new Rectangle(pattern.X[i], pattern.Y[i], width, height);
                    patternData.Frames = frames;

                    AddPattern(id, patternData);
                }
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to load tileset: {0}".F(ex.Message));
                return false;
            }
            return true;
        }

        private bool AddPattern(string patternId, TilePatternData pattern)
        {
            if (_patterns.ContainsKey(patternId))
                return false;
            
            _patterns.Add(patternId, pattern);
            return true;
        }
    }

    [XmlRoot("Tileset")]
    public class TilesetXmlData
    {
        public class TilePatternXmlData
        {
            [XmlAttribute]
            public string Id { get; set; }
            public Ground? Ground { get; set; }
            public Layer? DefaultLayer { get; set; }
            public TileScrolling? Scrolling { get; set; }
            [XmlElement("X")]
            public int[] X { get; set; }
            [XmlElement("Y")]
            public int[] Y { get; set; }
            public int? Width { get; set; }
            public int? Height { get; set; }
        }

        public ColorXmlData BackgroundColor { get; set; }

        [XmlElement("TilePattern")]
        public TilePatternXmlData[] TilePatterns { get; set; }
    }
}
