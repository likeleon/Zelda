﻿using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class FreeState : PlayerMovementState
    {
        public FreeState(Hero hero)
            : base(hero, "free")
        {
        }

        public override void Stop(State nextState)
        {
            base.Stop(nextState);

            CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
        }

        public override void SetAnimationStopped()
        {
            Sprites.SetAnimationStoppedNormal();
        }

        public override void SetAnimationWalking()
        {
            Sprites.SetAnimationWalkingNormal();
        }
    }
}
