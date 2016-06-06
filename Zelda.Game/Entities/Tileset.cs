using System;
using System.Collections.Generic;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    class Tileset
    {
        public string Id { get; }
        public Color BackgroundColor { get; }
        public Surface TilesImage { get; }
        public Surface EntitiesImage { get; }
        public bool IsLoaded => TilesImage != null;

        readonly Dictionary<string, TilePattern> _tilePatterns = new Dictionary<string, TilePattern>();

        public Tileset(string id)
        {
            Id = id;

            // 타일셋 데이터 파일을 읽습니다
            var fileName = "tilesets/" + Id + ".xml";
            var data = XmlLoader.Load<TilesetData>(Core.Mod.ModFiles, fileName);
            BackgroundColor = data.BackgroundColor;
            data.Patterns.Do(x => AddTilePattern(x.Key, x.Value));

            // 타일셋 이미지들을 읽습니다
            fileName = "tilesets/" + Id + ".tiles.png";
            TilesImage = Surface.Create(fileName, false, Surface.ImageDirectory.Data);
            if (TilesImage == null)
                throw new Exception("Missing tiles image for tileset '{0}': {1}".F(Id, fileName));

            fileName = "tilesets/" + Id + ".entities.png";
            EntitiesImage = Surface.Create(fileName, false, Surface.ImageDirectory.Data);
            if (EntitiesImage == null)
                throw new Exception("Missing entities image for tileset '{0}': {1}".F(Id, fileName));
        }

        void AddTilePattern(string id, TilePatternData p)
        {
            TilePattern tilePattern = null;

            if (p.Frames.Length == 1)
            {
                var frame = p.Frames[0];
                switch (p.Scrolling)
                {
                    case TileScrolling.None:
                        tilePattern = new SimpleTilePattern(p.Ground, frame.XY, frame.Size);
                        break;

                    case TileScrolling.Parallax:
                        tilePattern = new ParallaxScrollingTilePattern(p.Ground, frame.XY, frame.Size);
                        break;

                    case TileScrolling.Self:
                        tilePattern = new SelfScrollingTilePattern(p.Ground, frame.XY, frame.Size);
                        break;
                }
            }
            else
            {
                if (p.Scrolling == TileScrolling.Self)
                    throw new Exception("Multi-frame is not supported for self-scrolling tiles");

                bool parallax = p.Scrolling == TileScrolling.Parallax;
                var sequence = (p.Frames.Length == 3) ? AnimatedTilePattern.AnimationSequence.Sequence012 : AnimatedTilePattern.AnimationSequence.Sequence0121;
                tilePattern = new AnimatedTilePattern(p.Ground, sequence, p.Frames[0].Size, p.Frames[0].XY, p.Frames[1].XY, p.Frames[2].XY, parallax);
            }

            _tilePatterns.Add(id, tilePattern);
        }

        public TilePattern GetTilePattern(string id)
        {
            TilePattern pattern;
            if (!_tilePatterns.TryGetValue(id, out pattern))
                throw new ArgumentException("No such tile pattern in tileset '{0}': {1}".F(Id, id), nameof(id));

            return pattern;
        }
    }
}
