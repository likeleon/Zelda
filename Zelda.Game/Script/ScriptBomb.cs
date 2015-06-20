using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptBomb : ScriptEntity
    {
        internal Bomb Bomb { get; private set; }

        internal ScriptBomb(Bomb bomb)
            : base(bomb)
        {
            Bomb = bomb;
        }
    }
}
