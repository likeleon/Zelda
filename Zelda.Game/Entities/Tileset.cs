using Zelda.Game.Engine;

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
            _tilesImage.Dispose();
            _tilesImage = null;
            _entitiesImage.Dispose();
            _entitiesImage = null;
        }
    }
}
