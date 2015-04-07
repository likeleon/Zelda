using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Zelda.Game
{
    class SpriteAnimationDirectionData
    {
        readonly Point _xy;
        public Point XY
        {
            get { return _xy; }
        }

        readonly Size _size;
        public Size Size
        {
            get { return _size; }
        }

        readonly Point _origin;
        public Point Origin
        {
            get { return _origin; }
        }
        
        int _numFrames;
        public int NumFrames
        {
            get { return _numFrames; }
            set { _numFrames = Math.Max(value, 1); }
        }

        int _numColumns;
        public int NumColumns
        {
            get { return _numColumns; }
            set { _numColumns = Math.Max(value, 1); }
        }

        public SpriteAnimationDirectionData(Point xy, Size size, Point origin, int numFrames, int numColumns = 1)
        {
            _xy = xy;
            _size = size;
            _origin = origin;
            NumFrames = numFrames;
            NumColumns = numColumns;
        }

        public List<Rectangle> GetAllFrames()
        {
            List<Rectangle> frames = new List<Rectangle>();
            int frameNumber = 0;

            int numRows = _numFrames / _numColumns;
            if (_numFrames % _numColumns != 0)
                ++numRows;

            for (int row = 0; row < numRows && frameNumber < _numFrames; ++row)
            {
                for (int col = 0; col < _numColumns && frameNumber < NumFrames; ++col)
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
        readonly string _srcImage;
        public string SrcImage
        {
            get { return _srcImage; }
        }

        readonly uint _frameDelay;
        public uint FrameDelay
        {
            get { return _frameDelay; }
        }

        readonly int _loopOnFrame;
        public int LoopOnFrame
        {
            get { return _loopOnFrame; }
        }

        readonly List<SpriteAnimationDirectionData> _directions;
        public IEnumerable<SpriteAnimationDirectionData> Directions
        {
            get { return _directions; }
        }

        public SpriteAnimationData(
            string srcImage, 
            List<SpriteAnimationDirectionData> directions, 
            uint frameDelay, 
            int loopOnFrame)
        {
            _srcImage = srcImage;
            _directions = directions;
            _frameDelay = frameDelay;
            _loopOnFrame = loopOnFrame;
        }
    }

    class SpriteData : XmlData
    {
        Dictionary<string, SpriteAnimationData> _animations = new Dictionary<string, SpriteAnimationData>();
        public IDictionary<string, SpriteAnimationData> Animations
        {
            get { return _animations; }
        }
        
        string _defaultAnimationName;
        public string DefaultAnimationName
        {
            get { return _defaultAnimationName; }
        }

        protected override bool ImportFromStream(Stream stream)
        {
            try
            {
                SpriteAnimationSetXmlData xmlData = stream.XmlDeserialize<SpriteAnimationSetXmlData>();
                foreach (SpriteAnimationSetXmlData.Animation animation in xmlData.Animations)
                {
                    string animationName = animation.Name.CheckField("Name");
                    string srcImage = animation.SrcImage.CheckField("SrcImage");
                    uint frameDelay = (uint)animation.FrameDelay.OptField(0);
                    int frameToLoopOn = animation.FrameToLoopOn.OptField(-1);

                    if (frameToLoopOn < -1)
                        ScriptTools.ArgError("FrameToLoopOn", "Bad field 'FrameToLoopOn' (must be a positive number or -1)");

                    List<SpriteAnimationDirectionData> directions = new List<SpriteAnimationDirectionData>();
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
                            ScriptTools.ArgError("NumColumns", "Bad field 'NumColumns': must be between 1 and the number of frames");

                        if (frameToLoopOn >= numFrames)
                            ScriptTools.ArgError("FrameToLoopOn", "Bad field 'FrameToLoopOn': exceeds the number of frames");

                        directions.Add(new SpriteAnimationDirectionData(
                            new Point(x, y), 
                            new Size(frameWidth, frameHeight),
                            new Point(originX, originY),
                            numFrames,
                            numColumns));
                    }

                    if (_animations.ContainsKey(animationName))
                        Debug.Error("Duplicate animation '{0}'".F(animationName));

                    _animations.Add(animationName, new SpriteAnimationData(srcImage, directions, frameDelay, frameToLoopOn));

                    // 첫번째 애니메이션을 기본으로 해줍니다
                    if (_animations.Count == 1)
                        _defaultAnimationName = animationName;
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

    [XmlRoot("AnimationSet")]
    public class SpriteAnimationSetXmlData
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

            [XmlArrayItem("Direction")]
            public AnimationDirection[] Directions { get; set; }

            public bool FrameDelaySpecified { get { return FrameDelay != null; } }
            public bool FrameToLoopOnSpecified { get { return FrameToLoopOn != null; } }
        }

        [XmlElement("Animation")]
        public Animation[] Animations { get; set; }
    }
}
