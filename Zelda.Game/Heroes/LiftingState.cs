using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class LiftingState : State
    {
        CarriedItem _liftedItem;

        public LiftingState(Hero hero, CarriedItem liftedItem)
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
        }

        public override void Stop(State nextState)
        {
            base.Stop(nextState);

            if (_liftedItem != null)
            {
                Sprites.LiftedItem = null;

                switch (nextState.PreviousCarriedItemBehavior)
                {
                    case CarriedItem.Behavior.Throw:
                        ThrowItem();
                        break;

                    case CarriedItem.Behavior.Destroy:
                    case CarriedItem.Behavior.Keep:
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
                CarriedItem carriedItem = _liftedItem;
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

        public override bool CanBeHurt(MapEntity attacker)
        {
            return true;
        }
    }
}
