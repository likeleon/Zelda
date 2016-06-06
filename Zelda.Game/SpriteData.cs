using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    public class SpriteAnimationDirectionData : IXmlDeserialized
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }

        [DefaultValue(0)]
        public int OriginX { get; set; } = 0;
        [DefaultValue(0)]
        public int OriginY { get; set; } = 0;

        [XmlIgnore] public Point XY { get; private set; }
        [XmlIgnore] public Size Size { get; private set; }
        [XmlIgnore] public Point Origin { get; private set; }
        
        [DefaultValue(1)]
        public int NumFrames
        {
            get { return _numFrames; }
            set { _numFrames = Math.Max(value, 1); }
        }

        [DefaultValue(1)]
        public int NumColumns
        {
            get { return _numColumns; }
            set { _numColumns = Math.Max(value, 1); }
        }

        int _numFrames = 1;
        int _numColumns = 0;

        public List<Rectangle> GetAllFrames()
        {
            var frames = new List<Rectangle>();
            var frameNumber = 0;

            var numRows = _numFrames / _numColumns;
            if (_numFrames % _numColumns != 0)
                ++numRows;

            for (var row = 0; row < numRows && frameNumber < _numFrames; ++row)
                for (var col = 0; col < _numColumns && frameNumber < NumFrames; ++col)
                    frames.Add(new Rectangle(XY.X + col * Size.Width, XY.Y + row * Size.Height, Size.Width, Size.Height));

            return frames;
        }

        public void OnDeserialized()
        {
            if (NumColumns == 0)
                NumColumns = NumFrames;

            if (NumColumns < 1 || NumColumns > NumFrames)
                throw new InvalidDataException("Bad field 'NumColumns': must be between 1 and the number of frames");

            XY = new Point(X, Y);
            Size = new Size(FrameWidth, FrameHeight);
            Origin = new Point(OriginX, OriginY);
        }
    }

    public class SpriteAnimationData : IXmlDeserialized
    {
        [XmlAttribute]
        public string Name { get; set; }
        public string SrcImage { get; set; }

        [XmlIgnore]
        public bool SrcImageIsTileset => SrcImage == "tileset";

        [DefaultValue(0)]
        public int FrameDelay { get; set; } = 0;

        [DefaultValue(-1)]
        [XmlElement("FrameToLoopOn")]
        public int LoopOnFrame { get; set; } = -1;

        [XmlElement("Direction")]
        public SpriteAnimationDirectionData[] Directions { get; set; }

        public void OnDeserialized()
        {
            if (LoopOnFrame < -1)
                throw new InvalidDataException("Bad field 'FrameToLoopOn' (must be a positive number or -1");

            foreach (var direction in Directions.EmptyIfNull())
            {
                direction.OnDeserialized();

                if (LoopOnFrame >= direction.NumFrames)
                    throw new InvalidDataException("Bad field 'FrameToLoopOn': exceeds the number of frames");
            }
        }
    }

    [XmlRoot("Sprite")]
    public class SpriteData : IXmlDeserialized
    {
        [XmlElement("Animation")]
        public SpriteAnimationData[] AnimationDatas { get; set; }

        [XmlIgnore]
        public IReadOnlyDictionary<string, SpriteAnimationData> Animations => _animations;

        [XmlIgnore]
        public string DefaultAnimationName { get; private set; }

        Dictionary<string, SpriteAnimationData> _animations;

        public void OnDeserialized()
        {
            AnimationDatas.Do(a => a.OnDeserialized());
            _animations = AnimationDatas.ToDictionary(a => a.Name, a => a);

            // 첫번째 애니메이션을 기본으로 해줍니다
            DefaultAnimationName = AnimationDatas[0].Name;
        }
    }
}
