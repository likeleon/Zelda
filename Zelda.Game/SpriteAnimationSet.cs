using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    [XmlRoot("AnimationSet")]
    public class SpriteAnimationSetData
    {
        public class Direction
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
            public string Name { get; set; }
            public string SrcImage { get; set; }
            public int? FrameDelay { get; set; }
            public int? FrameToLoopOn { get; set; }
            public Direction[] Directions { get; set; }

            public bool FrameDelaySpecified { get { return FrameDelay != null; } }
            public bool FrameToLoopOnSpecified { get { return FrameToLoopOn != null; } }
        }

        [XmlElement("Animation")]
        public Animation[] Animations { get; set; }
    }

    class SpriteAnimationSet
    {
        string _defaultAnimationName;   // 기본 애니메이션의 이름
        public string DefaultAnimation
        {
            get { return _defaultAnimationName; }
        }

        Size _maxSize;   // 가장 큰 프레임의 크기
        public Size MaxSize
        {
            get { return _maxSize; }
        }

        readonly string _id;
        readonly Dictionary<string, SpriteAnimation> _animations = new Dictionary<string, SpriteAnimation>();

        public SpriteAnimationSet(string id)
        {
            _id = id;

            Load();
        }

        void Load()
        {
            if (_animations.Count > 0)
                throw new Exception("Animation set already loaded");

            string fileName = "Sprites/" + _id + ".xml";

            using (MemoryStream stream = ModFiles.DataFileRead(fileName))
            {
                SpriteAnimationSetData xmlData = stream.XmlDeserialize<SpriteAnimationSetData>();
                foreach (SpriteAnimationSetData.Animation animation in xmlData.Animations)
                {
                    string animationName = CheckStringField(animation.Name, "Name");
                    string srcImage = CheckStringField(animation.SrcImage, "SrcImage");
                    uint frameDelay = (uint)OptIntField(animation.FrameDelay, 0);
                    int frameToLoopOn = OptIntField(animation.FrameToLoopOn, -1);

                    if (frameToLoopOn < -1)
                        throw new Exception("Bad field 'FrameToLoopOn' (must be a positive number or -1)");

                    SpriteAnimationDirection[] directions = new SpriteAnimationDirection[animation.Directions.Length];
                    for (int i = 0; i < animation.Directions.Length; ++i)
                    {
                        SpriteAnimationSetData.Direction direction = animation.Directions[i];
                        int x = CheckIntField(direction.X, "X");
                        int y = CheckIntField(direction.Y, "Y");
                        int frameWidth = CheckIntField(direction.FrameWidth, "FrameWidth");
                        int frameHeight = CheckIntField(direction.FrameHeight, "FrameHeight");
                        int originX = OptIntField(direction.OriginX, 0);
                        int originY = OptIntField(direction.OriginY, 0);
                        int numFrames = OptIntField(direction.NumFrames, 1);
                        int numColumns = OptIntField(direction.NumColumns, numFrames);

                        if (numColumns < 1 || numColumns > numFrames)
                            throw new Exception("Bad field 'NumColumns': must be between 1 and the number of frames");

                        if (frameToLoopOn >= numFrames)
                            throw new Exception("Bad field 'FrameToLoopOn': exceeds the number of frames");

                        int maxWidth = Math.Max(frameWidth, _maxSize.Width);
                        int maxHeight = Math.Max(frameHeight, _maxSize.Height);
                        _maxSize = new Size(maxWidth, maxHeight);

                        int numRows;
                        if (numFrames % numColumns == 0)
                            numRows = numFrames / numColumns;
                        else
                            numRows = (numFrames / numColumns) + 1;

                        Rectangle[] positionsInSrc = new Rectangle[numFrames];
                        int j = 0;  // frame number.
                        for (int r = 0; r < numRows && j < numFrames; ++r)
                        {
                            for (int c = 0; c < numColumns && j < numFrames; ++c)
                            {
                                Rectangle positionInSrc = new Rectangle(
                                    x + c * frameWidth,
                                    y + r * frameHeight,
                                    frameWidth,
                                    frameHeight);
                                positionsInSrc[j] = positionInSrc;
                                ++j;
                            }
                        }

                        directions[i] = new SpriteAnimationDirection(positionsInSrc, new Point(originX, originY));
                    }

                    if (_animations.ContainsKey(animationName))
                    {
                        string msg = String.Format("Duplicate animation '{0}' in sprite '{1}'", animationName, _id);
                        throw new Exception(msg);
                    }

                    _animations.Add(animationName, new SpriteAnimation(srcImage, directions, frameDelay, frameToLoopOn));

                    // 첫번째 애니메이션을 기본으로 해줍니다
                    if (_animations.Count == 1)
                        _defaultAnimationName = animationName;
                }
            }
        }

        string CheckStringField(string value, string name)
        {
            if (value == null)
                throw new Exception(String.Format("Bad field '{0}' (non-null string expected)", name));
            
            return value;
        }

        int CheckIntField(int? value, string name)
        {
            if (!value.HasValue)
                throw new Exception(String.Format("Bad field '{0}' (non-null int expected)", name));

            return value.Value;
        }

        int OptIntField(int? value, int defaultValue)
        {
            if (value.HasValue)
                return value.Value;
            else
                return defaultValue;
        }

        public bool HasAnimation(string animationName)
        {
            return _animations.ContainsKey(animationName);
        }

        public SpriteAnimation GetAnimation(string animationName)
        {
            if (!HasAnimation(animationName))
                throw new Exception("No animation '" + animationName + "' in animation set '" + _id + "'");

            return _animations[animationName];
        }
    }
}
