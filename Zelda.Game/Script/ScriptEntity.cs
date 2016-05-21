using Zelda.Game.LowLevel;
using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public abstract class ScriptEntity
    {
        readonly MapEntity _entity;

        public Point Position => _entity.XY;
        public Layer Layer => _entity.Layer;
        public Sprite Sprite => _entity.Sprite;
        public bool IsEnabled => _entity.IsEnabled;

        internal ScriptEntity(MapEntity entity)
        {
            _entity = entity;
        }

        public void SetEnabled(bool enabled)
        {
            ScriptToCore.Call(() => _entity.SetEnabled(enabled));
        }
    }
}
