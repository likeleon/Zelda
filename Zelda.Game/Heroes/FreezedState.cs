using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class FreezedState : State
    {
        public FreezedState(Hero hero)
            : base(hero, "freezed")
        {
        }

        public override void Start(State previousState)
        {
            base.Start(previousState);

            Sprites.SetAnimationStoppedNormal();
            CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
        }
    }
}
