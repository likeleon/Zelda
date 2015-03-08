using System;
using System.Collections.Generic;
using System.ComponentModel;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class Sprite : Drawable
    {
        static readonly Dictionary<string, SpriteAnimationSet> _allAnimationSets = new Dictionary<string, SpriteAnimationSet>();

        readonly string _animationSetId;    // 이 스프라이트의 애니메이션 셋 아이디
        public string AnimationSetId
        {
            get { return _animationSetId; }
        }

        readonly SpriteAnimationSet _animationSet;
        public SpriteAnimationSet AnimationSet
        {
            get { return _animationSet; }
        }

        bool _finished;
        public bool IsAnimationFinished
        {
            get { return _finished; }
        }

        public bool IsAnimationStarted
        {
            get { return !IsAnimationFinished; }
        }

        uint _frameDelay;   // 두 프레임간 딜레이 (밀리초)
        uint FrameDelay
        {
            get { return _frameDelay; }
            set { _frameDelay = value; }
        }

        int _currentFrame = -1;
        public int CurrentFrame
        {
            get { return _currentFrame; }
        }

        bool _frameChanged;
        public bool FrameChanged
        {
            get { return _frameChanged; }
            set { _frameChanged = value; }
        }

        int _currentDirection;
        public int CurrentDirection
        {
            get { return _currentDirection; }
        }

        [Description("현재 애니메이션의 방향 개수")]
        public int NumDirections
        {
            get { return _currentAnimation.NumDirections; }
        }

        string _currentAnimationName;
        [Description("현재 애니메이션")]
        public string CurrentAnimation
        {
            get { return _currentAnimationName; }
        }

        SpriteAnimation _currentAnimation;
        uint nextFrameDate;        

        public static void Initialize()
        {
        }

        public static void Quit()
        {
            _allAnimationSets.Clear();
        }

        public Sprite(string id)
        {
            _animationSetId = id;
            _animationSet = GetAnimationSet(id);
            SetCurrentAnimation(_animationSet.DefaultAnimation);
        }

        static SpriteAnimationSet GetAnimationSet(string id)
        {
            SpriteAnimationSet animationSet;
            if (!_allAnimationSets.TryGetValue(id, out animationSet))
            {
                animationSet = new SpriteAnimationSet(id);
                _allAnimationSets.Add(id, animationSet);
            }

            if (animationSet == null)
                throw new Exception("No animation set");

            return animationSet;
        }

        public override void RawDraw(Surface dstSurface, Point dstPosition)
        {
            if (!IsAnimationFinished)
                _currentAnimation.Draw(dstSurface, dstPosition, _currentDirection, _currentFrame);
        }

        public override void RawDrawRegion(Rectangle region, Surface dstSurface, Point dstPosition)
        {
            throw new NotImplementedException();
        }

        public void SetCurrentAnimation(string animationName)
        {
            if (animationName != _currentAnimationName || !IsAnimationStarted)
            {
                _currentAnimationName = animationName;
                _currentAnimation = _animationSet.GetAnimation(animationName);
                FrameDelay = _currentAnimation.FrameDelay;

                SetCurrentFrame(0);
            }
        }

        public void SetCurrentFrame(int currentFrame)
        {
            _finished = false;
            nextFrameDate = EngineSystem.Now + FrameDelay;

            if (currentFrame != _currentFrame)
            {
                _currentFrame = currentFrame;
                FrameChanged = true;
            }
        }

        public void RestartAnimation()
        {
            SetCurrentFrame(0);
        }

        public void SetCurrentDirection(int currentDirection)
        {
            if (currentDirection < 0 || currentDirection >= NumDirections)
            {
                string msg = "Invalid direction {0} for sprite '{1}' in animation {2}".F(currentDirection, AnimationSetId, _currentAnimationName);
                throw new ArgumentOutOfRangeException("currentDirection", msg);
            }

            if (currentDirection == _currentDirection)
                return;

            _currentDirection = currentDirection;

            SetCurrentFrame(0);
        }
    }
}
