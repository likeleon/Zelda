using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    class AnimatedTilePattern : TilePattern
    {
        public enum AnimationSequence
        {
            Sequence012 = 1,
            Sequence0121 = 2
        }

        public override bool IsDrawnAtItsPosition => !_parallax;

        static int _frameCounter;
        static int[] _currentFrames = new int[3] { 0, 0, 0 };
        static int _nextFrameDate;
        static readonly int _tileFrameInterval = 250;
        static readonly int[,] _frames = new int[,]
        {
            { 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2 },
            { 0, 1, 2, 1, 0, 1, 2, 1, 0, 1, 2, 1 }
        };

        readonly AnimationSequence _sequence;
        readonly bool _parallax;
        readonly Rectangle[] _positionInTileset = new Rectangle[3];

        public AnimatedTilePattern(Ground ground, AnimationSequence sequence, Size size, Point xy1, Point xy2, Point xy3, bool parallax)
            : base(ground, size)
        {
            _sequence = sequence;
            _parallax = parallax;

            _positionInTileset[0] = new Rectangle(xy1, size);
            _positionInTileset[1] = new Rectangle(xy2, size);
            _positionInTileset[2] = new Rectangle(xy3, size);
        }

        public new static void Update()
        {
            while (Core.Now >= _nextFrameDate)
            {
                _frameCounter = (_frameCounter + 1) % 12;
                _currentFrames[1] = _frames[0, _frameCounter];
                _currentFrames[2] = _frames[1, _frameCounter];

                _nextFrameDate += _tileFrameInterval;  // 매 250밀리초마다 프레임을 변화시킵니다
            }
        }

        public override void Draw(Surface dstSurface, Point dstPosition, Tileset tileset, Point viewport)
        {
            var src = _positionInTileset[_currentFrames[(int)_sequence]];
            var dst = dstPosition;

            if (_parallax)
                dst += viewport / ParallaxScrollingTilePattern.Ratio;

            tileset.TilesImage.DrawRegion(src, dstSurface, dst);
        }
    }
}
