using Zelda.Game.Entities;
using RawMap = Zelda.Game.Map;

namespace Zelda.Game.Script
{
    public class Map
    {
        internal RawMap RawMap { get; private set; }

        internal void NotifyStarted(RawMap rawMap, Destination destionation)
        {
            RawMap = rawMap;

            ScriptTools.ExceptionBoundaryHandle(() => { OnStarted(); });
        }

        public ScriptEntity GetEntity(string name)
        {
            MapEntity entity = RawMap.Entities.FindEntity(name);
            return (entity != null) ? entity.ScriptEntity : null;
        }

        protected virtual void OnStarted()
        {
        }
    }
}
