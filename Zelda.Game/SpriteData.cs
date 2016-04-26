using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    class SpriteAnimationDirectionData
    {
        public Point XY { get; private set; }
        public Size Size { get; private set; }
        public Point Origin { get; private set; }
        
        public int NumFrames
        {
            get { return _numFrames; }
            set { _numFrames = Math.Max(value, 1); }
        }

        public int NumColumns
        {
            get { return _numColumns; }
            set { _numColumns = Math.Max(value, 1); }
        }

        int _numFrames;
        int _numColumns;

        public SpriteAnimationDirectionData(Point xy, Size size, Point origin, int numFrames, int numColumns = 1)
        {
            XY = xy;
            Size = size;
            Origin = origin;
            NumFrames = numFrames;
            NumColumns = numColumns;
        }

        public List<Rectangle> GetAllFrames()
        {
            var frames = new List<Rectangle>();
            var frameNumber = 0;

            var numRows = _numFrames / _numColumns;
            if (_numFrames % _numColumns != 0)
                ++numRows;

            for (var row = 0; row < numRows && frameNumber < _numFrames; ++row)
            {
                for (var col = 0; col < _numColumns && frameNumber < NumFrames; ++col)
                {
                    frames.Add(new Rectangle(
                        XY.X + col * Size.Width,
                        XY.Y + row * Size.Height,
                        Size.Width,
                        Size.Height));
                }
            }

            return frames;
        }
    }

    class SpriteAnimationData
    {
        public string SrcImage { get; }
        public bool SrcImageIsTileset { get { return SrcImage == "tileset"; } }
        public uint FrameDelay { get; }
        public int LoopOnFrame { get; }
        public IEnumerable<SpriteAnimationDirectionData> Directions { get; }

        public SpriteAnimationData(string srcImage, List<SpriteAnimationDirectionData> directions, uint frameDelay, int loopOnFrame)
        {
            SrcImage = srcImage;
            Directions = directions;
            FrameDelay = frameDelay;
            LoopOnFrame = loopOnFrame;
        }
    }

    class SpriteData : XmlData
    {
        public IReadOnlyDictionary<string, SpriteAnimationData> Animations { get { return _animations; } }
        public string DefaultAnimationName { get; private set; }

        readonly Dictionary<string, SpriteAnimationData> _animations = new Dictionary<string, SpriteAnimationData>();

        protected override bool OnImportFromBuffer(byte[] buffer)
        {
            try
            {
                var xmlData = buffer.XmlDeserialize<SpriteXmlData>();
                foreach (var animation in xmlData.Animations)
                {
                    string animationName = animation.Name.CheckField("Name");
                    string srcImage = animation.SrcImage.CheckField("SrcImage");
                    uint frameDelay = (uint)animation.FrameDelay.OptField(0);
                    int frameToLoopOn = animation.FrameToLoopOn.OptField(-1);

                    if (frameToLoopOn < -1)
                        throw new InvalidDataException("Bad field 'FrameToLoopOn' (must be a positive number or -1");

                    var directions = new List<SpriteAnimationDirectionData>();
                    foreach (var direction in animation.Directions)
                    {
                        int x = direction.X.CheckField("X");
                        int y = direction.Y.CheckField("Y");
                        int frameWidth = direction.FrameWidth.CheckField("FrameWidth");
                        int frameHeight = direction.FrameHeight.CheckField("FrameHeight");
                        int originX = direction.OriginX.OptField(0);
                        int originY = direction.OriginY.OptField(0);
                        int numFrames = direction.NumFrames.OptField(1);
                        int numColumns = direction.NumColumns.OptField(numFrames);

                        if (numColumns < 1 || numColumns > numFrames)
                            throw new InvalidDataException("Bad field 'NumColumns': must be between 1 and the number of frames");

                        if (frameToLoopOn >= numFrames)
                            throw new InvalidDataException("Bad field 'FrameToLoopOn': exceeds the number of frames");

                        directions.Add(new SpriteAnimationDirectionData(
                            new Point(x, y), 
                            new Size(frameWidth, frameHeight),
                            new Point(originX, originY),
                            numFrames,
                            numColumns));
                    }

                    if (_animations.ContainsKey(animationName))
                        throw new InvalidDataException("Duplicate animation '{0}'".F(animationName));

                    _animations.Add(animationName, new SpriteAnimationData(srcImage, directions, frameDelay, frameToLoopOn));

                    // 첫번째 애니메이션을 기본으로 해줍니다
                    if (_animations.Count == 1)
                        DefaultAnimationName = animationName;
                }
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to load sprite: {0}".F(ex.Message));
                return false;
            }
            return true;
        }
    }

    [XmlRoot("Sprite")]
    public class SpriteXmlData
    {
        public class AnimationDirection
        {
            public int? X { get; set; }
            public int? Y { get; set; }
            public int? FrameWidth { get; set; }
            public int? FrameHeight { get; set; }
            public int? OriginX { get; set; }
            public int? OriginY { get; set; }
            public int? NumFrames { get; set; }
            public int? NumColumns { get; set; }

            public bool OriginXSpecified { get { return OriginX != null; } }
            public bool OriginYSpecified { get { return OriginY != null; } }
            public bool NumFramesSpecified { get { return NumFrames != null; } }
            public bool NumColumnsSpecified { get { return NumColumns != null; } }
        }

        public class Animation
        {
            [XmlAttribute]
            public string Name { get; set; }
            public string SrcImage { get; set; }
            public int? FrameDelay { get; set; }
            public int? FrameToLoopOn { get; set; }

            [XmlElement("Direction")]
            public AnimationDirection[] Directions { get; set; }

            public bool FrameDelaySpecified { get { return FrameDelay != null; } }
            public bool FrameToLoopOnSpecified { get { return FrameToLoopOn != null; } }
        }

        [XmlElement("Animation")]
        public Animation[] Animations { get; set; }
    }
}
