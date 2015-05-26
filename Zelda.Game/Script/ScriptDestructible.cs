using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptDestructible : ScriptEntity
    {
        readonly Destructible _destructible;
        internal Destructible Destructible
        {
            get { return _destructible; }
        }

        internal ScriptDestructible(Destructible destructible)
            : base(destructible)
        {
            _destructible = destructible;
        }
    }
}
