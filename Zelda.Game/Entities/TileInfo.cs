using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    class TileInfo
    {
        public Layer Layer { get; }
        public Rectangle Box { get; }
        public string PatternId { get; }
        public TilePattern Pattern { get; }

        public TileInfo(Layer layer, Rectangle box, string patternId, TilePattern pattern)
        {
            Layer = layer;
            Box = box;
            PatternId = patternId;
            Pattern = pattern;
        }
    }
}
