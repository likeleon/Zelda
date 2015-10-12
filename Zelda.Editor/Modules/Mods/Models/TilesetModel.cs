namespace Zelda.Editor.Modules.Mods.Models
{
    class TilesetModel
    {
        readonly IMod _mod;
        readonly string _tilesetId;
        readonly MapModel _mapModel;

        public TilesetModel(IMod mod, string tilesetId, MapModel mapModel)
        {
            _mod = mod;
            _tilesetId = tilesetId;
            _mapModel = mapModel;
        }
    }
}