using System;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class SpriteAnimationDirection
    {
        public int NumFrames
        {
            get { return _frames.Length; }
        }

        readonly Rectangle[] _frames;
        readonly Point _origin;

        public SpriteAnimationDirection(Rectangle[] frames, Point origin)
        {
            if (frames.Length <= 0)
                throw new ArgumentException("Empty sprite direction", "frames");

            _frames = frames;
            _origin = origin;
        }

        public void Draw(Surface dstSurface, Point dstPosition, int currentFrame, Surface srcImage)
        {
            Rectangle currentFrameRect = GetFrame(currentFrame);
            srcImage.DrawRegion(currentFrameRect, dstSurface, dstPosition - _origin);
        }

        public Rectangle GetFrame(int frame)
        {
            if (frame < 0 || frame >= NumFrames)
            {
                string msg = "Invalid frame {0}: this direction has {1} frames".F(frame, NumFrames);
                throw new ArgumentOutOfRangeException("frame", msg);
            }
            return _frames[frame];
        }
    }
}
