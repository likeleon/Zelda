using System;
using System.Collections.Generic;
using Zelda.Game;
using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Editor.Core.Mods
{
    class MapModel
    {
        public event EventHandler<string> TilesetIdChanged;
        public event EventHandler<Size> SizeChanged;

        public string MapId { get; private set; }

        readonly IMod _mod;
        readonly MapData _map = new MapData();
        TilesetModel _tilesetModel;
        readonly List<MapEntity>[] _entities = new List<MapEntity>[(int)Layer.NumLayer];

        public MapModel(IMod mod, string mapId)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");
            if (mapId == null)
                throw new ArgumentNullException("mapId");

            _mod = mod;
            MapId = mapId;

            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
                _entities[layer] = new List<MapEntity>();
        }

        public void SetTilesetId(string tilesetId)
        {
            if (tilesetId == _map.TilesetId)
                return;

            _map.TilesetId = tilesetId;
            if (!tilesetId.IsNullOrEmpty())
                _tilesetModel = new TilesetModel(_mod, tilesetId, this);

            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
                _entities[layer].ForEach(e => e.NotifyTilesetChanged());

            if (TilesetIdChanged != null)
                TilesetIdChanged(this, tilesetId);
        }

        public void SetSize(Size size)
        {
            if (size == _map.Size)
                return;

            _map.Size = size;
            if (SizeChanged != null)
                SizeChanged(this, size);
        }

        public void Save()
        {
            var path = _mod.GetMapDataFilePath(MapId);
            if (_map.ExportToFile(path))
                throw new Exception("Cannot save map data file '{0}'".F(path));
        }
    }
}
