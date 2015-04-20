using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class FreeState : PlayerMovementState
    {
        public FreeState(Hero hero)
            : base(hero, "free")
        {
        }
    }
}
