﻿using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Game.LowLevel;
using Zelda.Game.Entities;

namespace Zelda.Game
{
    class SpriteAnimationSet
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

        void Load()
        {
            Debug.CheckAssertion(_animations.Count <= 0, "Animation set already loaded");

            string fileName = "sprites/" + _id + ".xml";
            SpriteData data = new SpriteData();
            bool success = data.ImportFromModFile(Core.Mod.ModFiles, fileName);
            if (success)
            {
                DefaultAnimation = data.DefaultAnimationName;
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
                MaxSize = new Size(Math.Max(direction.Size.Width, MaxSize.Width), Math.Max(direction.Size.Height, MaxSize.Height));
                directions.Add(new SpriteAnimationDirection(direction.GetAllFrames().ToArray(), direction.Origin));
            }

            _animations.Add(animationName, new SpriteAnimation(srcImage, directions.ToArray(), frameDelay, frameToLoopOn));
        }

        public void EnablePixelCollisions()
        {
            if (!ArePixelCollisionsEnabled())
            {
                foreach (SpriteAnimation animation in _animations.Values)
                    animation.EnablePixelCollisions();
            }
        }

        public bool ArePixelCollisionsEnabled()
        {
            if (_animations.Count <= 0)
                return false;

            return _animations.First().Value.ArePixelCollisionsEnabled();
        }
    }
}
