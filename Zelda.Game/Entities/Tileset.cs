using System.Collections.Generic;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    class Tileset
    {
        readonly string _id;
        public string Id
        {
            get { return _id; }
        }

        Color _backgroundColor;
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
        }

        Surface _tilesImage;
        public Surface TilesImage
        {
            get { return _tilesImage; }
        }

        Surface _entitiesImage;
        public Surface EntitiesImage
        {
            get { return _entitiesImage; }
        }

        public bool IsLoaded
        {
            get { return _tilesImage != null; }
        }

        readonly Dictionary<string, TilePattern> _tilePatterns = new Dictionary<string, TilePattern>();

        public Tileset(string id)
        {
            _id = id;
        }

        public void Load()
        {
            // 타일셋 데이터 파일을 읽습니다
            string fileName = "tilesets/" + _id + ".xml";
            TilesetData data = new TilesetData();
            bool success = data.ImportFromModFile(fileName);
            if (success)
            {
                _backgroundColor = data.BackgroundColor;
                foreach (var pattern in data.Patterns)
                    AddTilePattern(pattern.Key, pattern.Value);
            }

            // 타일셋 이미지들을 읽습니다
            fileName = "tilesets/" + _id + ".tiles.png";
            _tilesImage = Surface.Create(fileName, Surface.ImageDirectory.Data);
            if (_tilesImage == null)
            {
                Debug.Error("Missing tiles image for tileset '{0}': {1}".F(_id, fileName));
                _tilesImage = Surface.Create(16, 16);
            }

            fileName = "tilesets/" + _id + ".entities.png";
            _entitiesImage = Surface.Create(fileName, Surface.ImageDirectory.Data);
            if (_entitiesImage == null)
            {
                Debug.Error("Missing entities image for tileset '{0}': {1}".F(_id, fileName));
                _entitiesImage = Surface.Create(16, 16);
            }
        }

        public void Unload()
        {
            _tilePatterns.Clear();
            _tilesImage.Dispose();
            _tilesImage = null;
            _entitiesImage.Dispose();
            _entitiesImage = null;
        }

        private void AddTilePattern(string id, TilePatternData patternData)
        {
            TilePattern tilePattern = null;

            Rectangle[] frames = patternData.Frames;
            TileScrolling scrolling = patternData.Scrolling;
            Ground ground = patternData.Ground;

            if (frames.Length == 1)
            {
                Rectangle frame = frames[0];
                switch (scrolling)
                {
                    case TileScrolling.None:
                        tilePattern = new SimpleTilePattern(ground, frame.XY, frame.Size);
                        break;

                    case TileScrolling.Parallax:
                        tilePattern = new ParallaxScrollingTilePattern(ground, frame.XY, frame.Size);
                        break;

                    case TileScrolling.Self:
                        tilePattern = new SelfScrollingTilePattern(ground, frame.XY, frame.Size);
                        break;
                }
            }
            else
            {
                if (scrolling == TileScrolling.Self)
                {
                    Debug.Error("Multi-frame is not supported for self-scrolling tiles");
                    return;
                }

                bool parallax = scrolling == TileScrolling.Parallax;
                AnimatedTilePattern.AnimationSequence sequence = (frames.Length == 3) ?
                    AnimatedTilePattern.AnimationSequence.Sequence012 : AnimatedTilePattern.AnimationSequence.Sequence0121;
                tilePattern = new AnimatedTilePattern(
                    ground,
                    sequence,
                    frames[0].Size,
                    frames[0].XY,
                    frames[1].XY,
                    frames[2].XY,
                    parallax);
            }

            _tilePatterns.Add(id, tilePattern);
        }

        public TilePattern GetTilePattern(string id)
        {
            TilePattern pattern;
            if (!_tilePatterns.TryGetValue(id, out pattern))
                Debug.Die("No such tile pattern in tileset '{0}': {1}".F(Id, id));

            return pattern;
        }
    }
}
