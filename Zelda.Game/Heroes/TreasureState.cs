using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class TreasureState : State
    {
        readonly Treasure _treasure;

        public TreasureState(Hero hero, Treasure treasure)
            : base(hero, "treasure")
        {
            _treasure = treasure;
            _treasure.CheckObtainable();
        }
    }
}
