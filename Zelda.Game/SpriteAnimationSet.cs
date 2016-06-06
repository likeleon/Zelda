using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Game.Entities;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    class SpriteAnimationSet : IDisposable
    {
        public string DefaultAnimation { get; private set; }
        public Size MaxSize { get; private set; }

        readonly string _id;
        readonly Dictionary<string, SpriteAnimation> _animations = new Dictionary<string, SpriteAnimation>();

        public SpriteAnimationSet(string id)
        {
            _id = id;

            Load();
        }

        public void Dispose()
        {
            _animations.Values.Do(a => a.Dispose());
        }

        void Load()
        {
            if (_animations.Count > 0)
                throw new InvalidOperationException("Animation set already loaded");

            var fileName = "sprites/" + _id + ".xml";
            var data = XmlLoader.Load<SpriteData>(Core.Mod.ModFiles, fileName);
            DefaultAnimation = data.DefaultAnimationName;
            data.Animations.Do(x => AddAnimation(x.Key, x.Value));
        }

        public void SetTileset(Tileset tileset)
        {
            _animations.Values.Do(a => a.SetTileset(tileset));
        }

        public bool HasAnimation(string animationName)
        {
            return _animations.ContainsKey(animationName);
        }

        public SpriteAnimation GetAnimation(string animationName)
        {
            if (!HasAnimation(animationName))
                throw new Exception("No animation '{0}' in animation set '{1}'".F(animationName, _id));

            return _animations[animationName];
        }

        void AddAnimation(string name, SpriteAnimationData a)
        {
            var directions = new List<SpriteAnimationDirection>();
            foreach (var direction in a.Directions)
            {
                MaxSize = new Size(Math.Max(direction.Size.Width, MaxSize.Width), Math.Max(direction.Size.Height, MaxSize.Height));
                directions.Add(new SpriteAnimationDirection(direction.GetAllFrames().ToArray(), direction.Origin));
            }

            _animations.Add(name, new SpriteAnimation(a.SrcImage, directions.ToArray(), a.FrameDelay, a.LoopOnFrame));
        }

        public void EnablePixelCollisions()
        {
            if (!ArePixelCollisionsEnabled())
                _animations.Values.Do(a => a.EnablePixelCollisions());
        }

        public bool ArePixelCollisionsEnabled()
        {
            if (_animations.Count <= 0)
                return false;

            return _animations.First().Value.ArePixelCollisionsEnabled();
        }
    }
}
