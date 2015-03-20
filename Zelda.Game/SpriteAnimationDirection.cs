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
            _frames = frames;
            _origin = origin;
            
            Debug.CheckAssertion(_frames != null && _frames.Length > 0, "Empty sprite direction");
        }

        public void Draw(Surface dstSurface, Point dstPosition, int currentFrame, Surface srcImage)
        {
            Rectangle currentFrameRect = GetFrame(currentFrame);
            srcImage.DrawRegion(currentFrameRect, dstSurface, dstPosition - _origin);
        }

        public Rectangle GetFrame(int frame)
        {
            if (frame < 0 || frame >= NumFrames)
                Debug.Die("Invalid frame {0}: this direction has {1} frames".F(frame, NumFrames));
            
            return _frames[frame];
        }
    }
}
