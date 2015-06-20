using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public abstract class ScriptEntity
    {
        readonly MapEntity _entity;

        public Point Position { get { return _entity.XY; } }
        public Layer Layer { get { return _entity.Layer; } }
        public ScriptSprite Sprite { get { return _entity.Sprite.ScriptSprite; } }
        public bool IsEnabled { get { return _entity.IsEnabled; } }

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
