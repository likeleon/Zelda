using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class CarryingState : PlayerMovementState
    {
        CarriedItem _carriedItem;

        public CarryingState(Hero hero, CarriedItem carriedItem)
            : base(hero, "carrying")
        {
            _carriedItem = carriedItem;
            Debug.CheckAssertion(_carriedItem != null, "Missing carried item");
        }

        public override void Start(State previousState)
        {
            base.Start(previousState);

            if (IsCurrentState)
            {
                Sprites.LiftedItem = _carriedItem;
                CommandsEffects.ActionCommandEffect = ActionCommandEffect.Throw;
            }
        }

        public override void Stop(State nextState)
        {
            base.Stop(nextState);

            Sprites.LiftedItem = null;
            CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;

            if (_carriedItem != null)
            {
                switch (nextState.PreviousCarriedItemBehavior)
                {
                    case CarriedItem.Behavior.Throw:
                        ThrowItem();
                        break;

                    case CarriedItem.Behavior.Destroy:
                    case CarriedItem.Behavior.Keep:
                        _carriedItem = null;
                        break;

                    default:
                        Debug.Die("Invalid carried item behavior");
                        break;
                }
            }
        }

        public override void SetMap(Map map)
        {
            base.SetMap(map);

            // 아이템을 들고 있는 도중에도 맵을 이동할 수 있습니다
            if (_carriedItem != null)
                _carriedItem.SetMap(map);
        }

        public override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (_carriedItem != null)
                _carriedItem.SetSuspended(suspended);
        }

        public override void Update()
        {
            base.Update();

            if (IsCurrentState)
            {
                _carriedItem.Update();
            }
        }

        public override void NotifyActionCommandPressed()
        {
            if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.Throw)
            {
                ThrowItem();
                Hero.SetState(new FreeState(Hero));
            }
        }

        void ThrowItem()
        {
        }

        public override void SetAnimationStopped()
        {
            Sprites.SetAnimationStoppedCarrying();
        }

        public override void SetAnimationWalking()
        {
            Sprites.SetAnimationWalkingCarrying();
        }

        public override CarriedItem CarriedItem
        {
            get { return _carriedItem; }
        }

        public override CarriedItem.Behavior PreviousCarriedItemBehavior
        {
            get { return CarriedItem.Behavior.Keep; }
        }
    }
}
