using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class CarryingState : PlayerMovementState
    {
        CarriedObject _carriedItem;

        public CarryingState(Hero hero, CarriedObject carriedItem)
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
                    case CarriedObject.Behavior.Throw:
                        ThrowItem();
                        break;

                    case CarriedObject.Behavior.Destroy:
                    case CarriedObject.Behavior.Keep:
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

                if (!IsSuspended)
                {
                    if (_carriedItem.IsBroken)
                    {
                        _carriedItem = null;
                        Hero.SetState(new FreeState(Hero));
                    }
                }
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
            _carriedItem.ThrowItem(Sprites.AnimationDirection);
            Entities.AddEntity(_carriedItem);
            _carriedItem = null;
        }

        public override void SetAnimationStopped()
        {
            Sprites.SetAnimationStoppedCarrying();
        }

        public override void SetAnimationWalking()
        {
            Sprites.SetAnimationWalkingCarrying();
        }

        public override CarriedObject CarriedItem
        {
            get { return _carriedItem; }
        }

        public override CarriedObject.Behavior PreviousCarriedItemBehavior
        {
            get { return CarriedObject.Behavior.Keep; }
        }

        public override void NotifyLayerChanged()
        {
            base.NotifyLayerChanged();

            if (_carriedItem != null)
                _carriedItem.SetLayer(Hero.Layer);
        }
    }
}
