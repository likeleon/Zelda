using System;
using Zelda.Game.Engine;
using Zelda.Game.Entities;

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

        Surface _srcImage;
        readonly bool _srcImageIsTileset;
        readonly int _loopOnFrame;
        readonly SpriteAnimationDirection[] _directions;

        public SpriteAnimation(
            string imageFileName,
            SpriteAnimationDirection[] directions,
            uint frameDelay,
            int loopOnframe)
        {
            _srcImageIsTileset = imageFileName == "tileset";
            _frameDelay = frameDelay;
            _directions = directions;
            _loopOnFrame = loopOnframe;

            if (!_srcImageIsTileset)
            {
                _srcImage = Surface.Create(imageFileName);
                Debug.CheckAssertion(_srcImage != null, "Cannot load image '{0}'".F(imageFileName));
            }
        }

        public void Dispose()
        {
            _srcImage.Dispose();
        }

        public void SetTileset(Tileset tileset)
        {
            if (!_srcImageIsTileset)
                return;

            _srcImage = tileset.EntitiesImage;
        }

        public void Draw(Surface dstSurface, Point dstPosition, int currentDirection, int currentFrame)
        {
            if (_srcImage == null)
                return;

            if (currentDirection < 0 || currentDirection >= NumDirections)
            {
                Debug.Die("Invalid sprite direction {0}: this sprite has {1} direction(s)"
                    .F(currentDirection, NumDirections));
            }

            _directions[currentDirection].Draw(dstSurface, dstPosition, currentFrame, _srcImage);
        }

        public int GetNextFrame(int currentDirection, int currentFrame)
        {
            if (currentDirection < 0 || currentDirection >= NumDirections)
            {
                string msg = "Invalid sprite direction '{0}': this sprite has {1} direction(s)"
                    .F(currentDirection, NumDirections);
                Debug.Die(msg);
            }

            int nextFrame = currentFrame + 1;

            // 마지막 프레임이라면
            if (nextFrame == _directions[currentDirection].NumFrames)
            {
                // 지정한 프레임에서 계속 루프를 돌게 하거나 루프가 없다면 -1을 사용하도록 합니다
                nextFrame = _loopOnFrame;
            }

            return nextFrame;
        }
    }
}
