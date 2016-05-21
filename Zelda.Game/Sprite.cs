using System;
using Zelda.Game.Entities;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;

namespace Zelda.Game
{
    class Sprite : Drawable, IDisposable
    {
        public Point Origin => _currentAnimation.GetDirection(CurrentDirection).Origin;
        public ScriptSprite ScriptSprite => _scriptSprite.Value;
        public string AnimationSetId { get; }
        public SpriteAnimationSet AnimationSet { get; }
        public bool IsAnimationFinished { get; private set; }
        public bool IsAnimationStarted => !IsAnimationFinished;
        public int CurrentFrame { get; private set; } = -1;
        public bool HasFrameChanged { get; set; }
        public Direction4 CurrentDirection { get; private set; }
        public int NumDirections => _currentAnimation.NumDirections;
        public string CurrentAnimation { get; private set; }
        public Size Size => _currentAnimation.GetDirection(CurrentDirection).Size;
        public Size MaxSize => AnimationSet.MaxSize;
        public bool IsBlinking => _blinkDelay != 0;
        public override Surface TransitionSurface => IntermediateSurface;

        Surface IntermediateSurface => _intermediateSurface.Value;

        readonly Lazy<Surface> _intermediateSurface;
        readonly Lazy<ScriptSprite> _scriptSprite;
        bool _ignoreSuspended;
        int _frameDelay;
        SpriteAnimation _currentAnimation;
        int _nextFrameDate;
        int _blinkDelay;
        bool _blinkIsSpriteVisible;
        int _blinkNextChangeDate;

        public Sprite(string id)
        {
            AnimationSetId = id;
            AnimationSet = Core.SpriteSystem.GetAnimationSet(id);
            SetCurrentAnimation(AnimationSet.DefaultAnimation);

            _intermediateSurface = Exts.Lazy(() => Surface.Create(MaxSize));
            _scriptSprite = Exts.Lazy(() => new ScriptSprite(this));
        }

        public void Dispose()
        {
            if (_scriptSprite.IsValueCreated)
                _scriptSprite.Value.Dispose();

            if (_intermediateSurface.IsValueCreated)
                _intermediateSurface.Value.Dispose();
        }

        public void EnablePixelCollisions()
        {
            AnimationSet.EnablePixelCollisions();
        }

        public bool ArePixelCollisionsEnabled()
        {
            return AnimationSet.ArePixelCollisionsEnabled();
        }

        public int GetNumFrames()
        {
            return _currentAnimation.GetDirection(CurrentDirection).NumFrames;
        }

        public void SetCurrentAnimation(string animationName)
        {
            if (animationName != CurrentAnimation || !IsAnimationStarted)
            {
                CurrentAnimation = animationName;
                _currentAnimation = AnimationSet.GetAnimation(animationName);
                _frameDelay = _currentAnimation.FrameDelay;

                SetCurrentFrame(0);
            }
        }

        public bool HasAnimation(string animationName)
        {
            return AnimationSet.HasAnimation(animationName);
        }

        public void SetCurrentFrame(int currentFrame)
        {
            IsAnimationFinished = false;
            _nextFrameDate = Core.Now + _frameDelay;

            if (currentFrame != CurrentFrame)
            {
                CurrentFrame = currentFrame;
                HasFrameChanged = true;
            }
        }

        public void RestartAnimation()
        {
            SetCurrentFrame(0);
        }

        public void SetCurrentDirection(Direction4 currentDirection)
        {
            if (currentDirection < 0 || (int)currentDirection >= NumDirections)
                throw new Exception("Invalid direction {0} for sprite '{1}' in animation {2}".F(currentDirection, AnimationSetId, CurrentAnimation));

            if (currentDirection == CurrentDirection)
                return;

            CurrentDirection = currentDirection;

            SetCurrentFrame(0);
        }

        public override void SetSuspended(bool suspended)
        {
            if (suspended == IsSuspended || _ignoreSuspended)
                return;

            base.SetSuspended(suspended);

            // 복귀라면 _nextFrameDate를 다시 계산해줍니다
            if (!suspended)
            {
                int now = Core.Now;
                _nextFrameDate = now + _frameDelay;
                _blinkNextChangeDate = now;
            }
            else
                _blinkIsSpriteVisible = true;
        }

        public void SetTileset(Tileset tileset)
        {
            AnimationSet.SetTileset(tileset);
        }

        public override void Update()
        {
            base.Update();

            if (IsSuspended)
                return;

            HasFrameChanged = false;
            int now = Core.Now;

            // 시간에 따라 프레임을 갱신해 줍니다
            int nextFrame = 0;
            while (!IsAnimationFinished && 
                   !IsSuspended && 
                   _frameDelay > 0 && 
                   now >= _nextFrameDate)
            {
                // 다음 프레임을 얻습니다
                nextFrame = GetNextFrame();

                // 애니메이션이 끝났는지 확인합니다
                if (nextFrame == -1)
                {
                    IsAnimationFinished = true;
                }
                else
                {
                    CurrentFrame = nextFrame;
                    _nextFrameDate += _frameDelay;
                }
                HasFrameChanged = true;
            }

            if (IsBlinking)
            {
                while (now >= _blinkNextChangeDate)
                {
                    _blinkIsSpriteVisible = !_blinkIsSpriteVisible;
                    _blinkNextChangeDate += _blinkDelay;
                }
            }
        }

        int GetNextFrame()
        {
            return _currentAnimation.GetNextFrame(CurrentDirection, CurrentFrame);
        }

        public override void RawDraw(Surface dstSurface, Point dstPosition)
        {
            if (!IsAnimationFinished &&
                (_blinkDelay == 0 || _blinkIsSpriteVisible))
            {
                if (!_intermediateSurface.IsValueCreated)
                    _currentAnimation.Draw(dstSurface, dstPosition, CurrentDirection, CurrentFrame);
                else
                {
                    IntermediateSurface.Clear();
                    _currentAnimation.Draw(IntermediateSurface, Origin, CurrentDirection, CurrentFrame);
                    IntermediateSurface.DrawRegion(new Rectangle(Size), dstSurface, dstPosition - Origin);
                }
            }
        }

        public override void RawDrawRegion(Rectangle region, Surface dstSurface, Point dstPosition)
        {
            throw new NotImplementedException();
        }

        public override void DrawTransition(Transition transition)
        {
            Transition.Draw(IntermediateSurface);
        }

        public void SetBlinking(int blinkDelay)
        {
            _blinkDelay = blinkDelay;

            if (blinkDelay > 0)
            {
                _blinkIsSpriteVisible = false;
                _blinkNextChangeDate = Core.Now;
            }
        }

        public bool TestCollision(Sprite other, int x1, int y1, int x2, int y2)
        {
            var direction1 = _currentAnimation.GetDirection(CurrentDirection);
            var origin1 = direction1.Origin;
            var location1 = new Point(x1 - origin1.X, y1 - origin1.Y);
            location1 += XY;
            var pixelBits1 = direction1.GetPixelBits(CurrentFrame);

            var direction2 = other._currentAnimation.GetDirection(other.CurrentDirection);
            var origin2 = direction2.Origin;
            var location2 = new Point(x2 - origin2.X, y2 - origin2.Y);
            location2 += other.XY;
            var pixelBits2 = direction2.GetPixelBits(other.CurrentFrame);

            return pixelBits1.TestCollision(pixelBits2, location1, location2);
        }

        public void SetIgnoreSuspended(bool ignoreSuspended)
        {
            SetSuspended(false);
            _ignoreSuspended = ignoreSuspended;
        }
    }
}
