using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public abstract class ScriptEntity
    {
        readonly MapEntity _entity;

        public Point Position
        {
            get { return _entity.XY; }
        }

        public Layer Layer
        {
            get { return _entity.Layer; }
        }

        internal ScriptEntity(MapEntity entity)
        {
            _entity = entity;
        }
    }
}
