using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptDestructible : ScriptEntity
    {
        internal Destructible Destructible { get; private set; }

        public Ground ModifiedGround { get { return Destructible.ModifiedGround; } }

        internal ScriptDestructible(Destructible destructible)
            : base(destructible)
        {
            Destructible = destructible;
        }
    }
}
