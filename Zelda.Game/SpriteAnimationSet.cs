using System;
using System.Collections.Generic;
using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game
{
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
            Debug.CheckAssertion(_animations.Count <= 0, "Animation set already loaded");

            string fileName = "sprites/" + _id + ".xml";
            SpriteData data = new SpriteData();
            bool success = data.ImportFromModFile(fileName);
            if (success)
            {
                _defaultAnimationName = data.DefaultAnimationName;
                foreach (var kvp in data.Animations)
                    AddAnimation(kvp.Key, kvp.Value);
            }
        }

        public void SetTileset(Tileset tileset)
        {
            foreach (SpriteAnimation animation in _animations.Values)
                animation.SetTileset(tileset);
        }

        public bool HasAnimation(string animationName)
        {
            return _animations.ContainsKey(animationName);
        }

        public SpriteAnimation GetAnimation(string animationName)
        {
            Debug.CheckAssertion(HasAnimation(animationName),
                "No animation '{0}' in animation set '{1}'".F(animationName, _id));

            return _animations[animationName];
        }

        void AddAnimation(string animationName, SpriteAnimationData animationData)
        {
            string srcImage = animationData.SrcImage;
            uint frameDelay = animationData.FrameDelay;
            int frameToLoopOn = animationData.LoopOnFrame;
            List<SpriteAnimationDirection> directions = new List<SpriteAnimationDirection>();

            foreach (var direction in animationData.Directions)
            {
                Size size = direction.Size;
                _maxSize.Width = Math.Max(size.Width, _maxSize.Width);
                _maxSize.Height = Math.Max(size.Height, _maxSize.Height);
                directions.Add(new SpriteAnimationDirection(direction.GetAllFrames().ToArray(), direction.Origin));
            }

            _animations.Add(animationName, new SpriteAnimation(srcImage, directions.ToArray(), frameDelay, frameToLoopOn));
        }
    }
}
