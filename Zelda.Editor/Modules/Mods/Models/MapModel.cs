﻿using System;
using System.Collections.Generic;
using Zelda.Game;
using Zelda.Game.LowLevel;
using Zelda.Game.Entities;

namespace Zelda.Editor.Modules.Mods.Models
{
    class MapModel
    {
        public event EventHandler<string> TilesetIdChanged;
        public event EventHandler<Size> SizeChanged;

        public string MapId { get; private set; }

        readonly IMod _mod;
        readonly MapData _map = new MapData();
        TilesetModel _tilesetModel;
        readonly List<EntityModel>[] _entities = new List<EntityModel>[(int)Layer.NumLayer];

        public MapModel(IMod mod, string mapId)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");
            if (mapId == null)
                throw new ArgumentNullException("mapId");

            _mod = mod;
            MapId = mapId;

            for (int layer = 0; layer < (int)Layer.NumLayer; ++layer)
                _entities[layer] = new List<EntityModel>();
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

            TilesetIdChanged?.Invoke(this, tilesetId);
        }

        public void SetSize(Size size)
        {
            if (size == _map.Size)
                return;

            _map.Size = size;
            SizeChanged?.Invoke(this, size);
        }

        public void Save()
        {
            XmlSaver.Save(_map, _mod.GetMapDataFilePath(MapId));
        }
    }
}
