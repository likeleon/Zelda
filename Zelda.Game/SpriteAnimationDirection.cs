using System;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class SpriteAnimationDirection
    {
        public SpriteAnimationDirection(Rectangle[] frames, Point origin)
        {
            _frames = frames;
            _origin = origin;
            
            Debug.CheckAssertion(_frames != null && _frames.Length > 0, "Empty sprite direction");
        }

        #region 크기와 중점
        public Size Size
        {
            get
            {
                Debug.CheckAssertion(NumFrames > 0, "Invalid number of frames");
                return new Size(_frames[0].Width, _frames[0].Height);
            }
        }

        readonly Point _origin;
        public Point Origin
        {
            get { return _origin; }
        }
        #endregion

        #region 프레임
        readonly Rectangle[] _frames;

        public int NumFrames
        {
            get { return _frames.Length; }
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
        #endregion

        #region 픽셀 충돌
        PixelBits[] _pixelBits;

        public bool ArePixelCollisionsEnabled
        {
            get { return _pixelBits != null; }
        }

        public void EnablePixelCollisions(Surface srcImage)
        {
            if (ArePixelCollisionsEnabled)
                return;
             
            _pixelBits = new PixelBits[NumFrames];
            for (int i = 0; i < NumFrames; ++i)
                _pixelBits[i] = new PixelBits(srcImage, _frames[i]);
        }
        
        public void DisablePixelCollisions()
        {
            _pixelBits = null;
        }

        public PixelBits GetPixelBits(int frame)
        {
            Debug.CheckAssertion(ArePixelCollisionsEnabled,
                "Pixel-precise collisions are not enabled for this sprite");
            Debug.CheckAssertion(frame >= 0 && frame < NumFrames, "Invalid frame number");
            return _pixelBits[frame];
        }
        #endregion
    }
}
