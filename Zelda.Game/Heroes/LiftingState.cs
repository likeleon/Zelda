﻿using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class LiftingState : State
    {
        CarriedObject _liftedItem;

        public LiftingState(Hero hero, CarriedObject liftedItem)
            : base(hero, "lifting")
        {
            _liftedItem = liftedItem;
            Debug.CheckAssertion(_liftedItem != null, "Missing lifted item");
        }

        public override void Start(State previousState)
        {
            base.Start(previousState);

            _liftedItem.SetMap(Map);

            CommandsEffects.ActionCommandEffect = ActionCommandEffect.Throw;
            Sprites.SetAnimationLifting();
            Sprites.LiftedItem = _liftedItem;
            Hero.FacingEntity = null;

            Equipment.NotifyAbilityUsed(Ability.Lift);
        }

        public override void Stop(State nextState)
        {
            base.Stop(nextState);

            if (_liftedItem != null)
            {
                Sprites.LiftedItem = null;

                switch (nextState.PreviousCarriedItemBehavior)
                {
                    case CarriedObject.Behavior.Throw:
                        ThrowItem();
                        break;

                    case CarriedObject.Behavior.Destroy:
                    case CarriedObject.Behavior.Keep:
                        _liftedItem = null;
                        break;
                }

                CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
            }
        }

        public override void Update()
        {
            base.Update();

            _liftedItem.Update();

            if (!IsSuspended && !_liftedItem.IsBeingLifted)
            {
                CarriedObject carriedItem = _liftedItem;
                _liftedItem = null;
                Hero.SetState(new CarryingState(Hero, carriedItem));
            }
        }

        public override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (_liftedItem != null)
                _liftedItem.SetSuspended(suspended);
        }

        void ThrowItem()
        {
            _liftedItem.ThrowItem(Sprites.AnimationDirection);
            Entities.AddEntity(_liftedItem);
            _liftedItem = null;
        }

        public override bool CanBeHurt(Entity attacker)
        {
            return true;
        }
    }
}
