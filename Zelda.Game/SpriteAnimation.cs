using System;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    // 애니메이션의 스프라이트 시퀀스를 저장힙니다
    // 각 시퀀스는 이 애니메이션에서의 스프라이트 방향입니다
    class SpriteAnimation : IDisposable
    {
        readonly uint _frameDelay;
        public uint FrameDelay
        {
            get { return _frameDelay; }
        }

        public bool IsLooping
        {
            get { return _loopOnFrame != -1; }
        }

        public int NumDirections
        {
            get { return _directions.Length; }
        }

        readonly Surface _srcImage;
        readonly int _loopOnFrame;
        readonly SpriteAnimationDirection[] _directions;

        public SpriteAnimation(
            string imageFileName,
            SpriteAnimationDirection[] directions,
            uint frameDelay,
            int loopOnframe)
        {
            _frameDelay = frameDelay;
            _directions = directions;
            _loopOnFrame = loopOnframe;

            _srcImage = Surface.Create(imageFileName);
            Debug.CheckAssertion(_srcImage != null, "Cannot load image '{0}'".F(imageFileName));
        }

        public void Draw(Surface dstSurface, Point dstPosition, int currentDirection, int currentFrame)
        {
            if (currentDirection < 0 || currentDirection >= NumDirections)
            {
                Debug.Die("Invalid sprite direction {0}: this sprite has {1} direction(s)"
                    .F(currentDirection, NumDirections));
            }

            if (_srcImage == null)
                return;

            _directions[currentDirection].Draw(dstSurface, dstPosition, currentFrame, _srcImage);
        }

        public void Dispose()
        {
            _srcImage.Dispose();
        }
    }
}
